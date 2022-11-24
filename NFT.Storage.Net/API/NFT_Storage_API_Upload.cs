using System.Text.Json;

namespace NFT.Storage.Net.API
{
    public partial class NFT_Storage_API
    {
        public bool UploadThrotteled { get; set;}
        /// <summary>
        /// this list is used to store th request times of the last 10 seconds (maximum 10 uploads per 10 seconds per api key)
        /// </summary>
        private Queue<DateTime> UploadRateLimitList = new Queue<DateTime>();
        /// <summary>
        /// this will prevent race conditions when rate throtteling in multithreaded access
        /// </summary>
        object lockObject = new object();
        /// <summary>
        /// This function ensures that the api rate limit of nft.storage is not exceeded
        /// </summary>
        private void AwaitRateLimit()
        {
            lock (lockObject)
            {
                // dequeue old (irrelevant entries)
                while (UploadRateLimitList.Any())
                {
                    DateTime requestTime;
                    if (UploadRateLimitList.TryPeek(out requestTime))
                    {
                        if (requestTime < DateTime.Now - GlobalVar.RateLimit_BacklogTimeSpan)
                        {
                            UploadRateLimitList.Dequeue();
                        }
                        else break;
                    }
                }
                // check if a rate limit applies
                if (UploadRateLimitList.Count >= GlobalVar.RateLimit_MaxRequests)
                {
                    // calculate sleep time
                    DateTime requestTime;
                    if (UploadRateLimitList.TryPeek(out requestTime))
                    {
                        TimeSpan sleep = requestTime - (DateTime.Now - GlobalVar.RateLimit_BacklogTimeSpan);
                        UploadThrotteled = true;
                        Task.Delay(sleep).Wait();
                    }
                }
                UploadRateLimitList.Enqueue(DateTime.Now);
                UploadThrotteled = false;
            }
        }
        /// <summary>
        /// Uploads a local file to NFT.Storage and returns a NFT_File object
        /// </summary>
        /// <param name="localPath"></param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="NotImplementedException">Files larger 100mb not supported by this library</exception>
        /// <exception cref="InvalidDataException">checksum invalid! Upload failed</exception>
        public async Task<NFT_File> Upload(string localPath)
        {
            return Upload(new FileInfo(localPath)).Result;
        }
        /// <summary>
        /// Uploads a local file to NFT.Storage and returns a NFT_File object
        /// </summary>
        /// <param name="localPath"></param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="NotImplementedException">Files larger 100mb not supported by this library</exception>
        /// <exception cref="InvalidDataException">checksum invalid! Upload failed</exception>
        public async Task<NFT_File> Upload(FileInfo localFile)
        {
            if (!localFile.Exists)
                throw new FileNotFoundException($"File not found! {localFile.FullName}");
            string checksum = Sha256.GetSha256Sum(localFile);
            using (Stream fs = localFile.OpenRead())
            {
                NFT_File file = Upload(fs).Result;
                file.Name = localFile.Name;
                if (checksum != file.Sha256Sum)
                {
                    throw new InvalidDataException("checksum invalid!");
                }
                return file;
            }
            throw new Exception("failed to openread the file");
        }
        /// <summary>
        /// Uploads a local file to NFT.Storage and returns a NFT_File object
        /// </summary>
        /// <param name="inputStream"></param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="NotImplementedException">Files larger 100mb not supported by this library</exception>
        public async Task<NFT_File> Upload(Stream inputStream)
        {
            if (inputStream.Length * 0.000001 > GlobalVar.MaxUploadSize_MB)
                throw new NotImplementedException("Files > 100 mb are not yet supported by this Library!");
            // rate limiter
            AwaitRateLimit();
            HttpResponseMessage response;
            //Load the file and set the file's Content-Type header
            using (StreamContent fileStreamContent = new StreamContent(inputStream))
            {
                //Send it
                response = await _Client.PostAsync("upload/", fileStreamContent);
            }
            response.EnsureSuccessStatusCode();
            // deserialize
            string responseJson = await response.Content.ReadAsStringAsync();
            ClientResponse.Response decodedResponse = JsonSerializer.Deserialize<ClientResponse.Response>(responseJson);
            // build return file
            NFT_File uploadedFile = new NFT_File(bulkUpload: false);
            uploadedFile.Name = decodedResponse.value.name;
            uploadedFile.Status = decodedResponse.value.pin.status;
            uploadedFile.Cid = decodedResponse.value.cid;
            uploadedFile.CalculateChecksum();
            return uploadedFile;
        }
    }
}
