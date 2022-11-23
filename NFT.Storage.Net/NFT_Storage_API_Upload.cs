using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NFT.Storage.Net
{
    public partial class NFT_Storage_API
    {
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
                        if (requestTime < DateTime.Now.AddSeconds(-10)) UploadRateLimitList.Dequeue();
                        else break;
                    }
                }
                // check if a rate limit applies
                if (UploadRateLimitList.Count >= 10)
                {
                    // calculate sleep time
                    DateTime requestTime;
                    if (UploadRateLimitList.TryPeek(out requestTime))
                    {
                        TimeSpan sleep = requestTime - DateTime.Now.AddSeconds(-10);
                        Task.Delay(sleep).Wait();
                    }
                }
                UploadRateLimitList.Enqueue(DateTime.Now);
            }
        }
        /// <summary>
        /// Uploads a local file to NFT.Storage and returns a NFT_File object
        /// </summary>
        /// <param name="localPath"></param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="NotImplementedException">Files larger 100mb not supported by this library</exception>
        public async Task<NFT_File> Upload(string localPath)
        {
            return Upload(new FileInfo(localPath)).Result;
        }
        public async Task<NFT_File> Upload(FileInfo localFile)
        {
            if (!localFile.Exists)
                throw new FileNotFoundException($"File not found! {localFile.FullName}");
            using (Stream fs = localFile.OpenRead())
            {
                NFT_File file = Upload(fs).Result;
                file.Name = localFile.Name;
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
            if (inputStream.Length * 0.000001 > 100)
                throw new NotImplementedException("Files > 100 mb are not yet supported by this Library!");
            // rate limiter
            AwaitRateLimit();
            using (var multipartFormContent = new MultipartFormDataContent())
            {
                HttpResponseMessage response;
                //Load the file and set the file's Content-Type header
                using (StreamContent fileStreamContent = new StreamContent(inputStream))
                {
                    fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                    //Send it
                    response = await _Client.PostAsync("upload/", fileStreamContent);
                }
                response.EnsureSuccessStatusCode();
                // deserialize
                string responseJson = await response.Content.ReadAsStringAsync();
                ClientResponse.Response decodedResponse = JsonSerializer.Deserialize<ClientResponse.Response>(responseJson);
                // build return file
                NFT_File uploadedFile = new NFT_File();
                uploadedFile.Name = decodedResponse.value.name;
                uploadedFile.Status = decodedResponse.value.pin.status;
                uploadedFile.Cid = decodedResponse.value.cid;
                return uploadedFile;
            }
            throw new NotImplementedException();
        }
    }
}
