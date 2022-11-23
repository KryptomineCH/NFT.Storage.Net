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
        /// <summary>
        /// Uploads a local file to NFT.Storage and returns a NFT_File object
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="NotImplementedException">Files larger 100mb not supported by this library</exception>
        public async Task<NFT_File> Upload(FileInfo fileInfo)
        {
            // prechecks
            if (!fileInfo.Exists)
                throw new FileNotFoundException($"Local File could not be found! {fileInfo.FullName}");
            if (fileInfo.Length * 0.000001 > 100)
                throw new NotImplementedException("Files > 100 mb are not yet supported by this Library!");
            // rate limiter
            AwaitRateLimit();
            using (var multipartFormContent = new MultipartFormDataContent())
            {
                //Load the file and set the file's Content-Type header
                var fileStreamContent = new StreamContent(fileInfo.OpenRead());
                fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                //Send it
                var response = await _Client.PostAsync("upload/", fileStreamContent);
                response.EnsureSuccessStatusCode();
                string responseJson = await response.Content.ReadAsStringAsync();
                fileStreamContent.Dispose();
                ClientResponse.Response decodedResponse = JsonSerializer.Deserialize<ClientResponse.Response>(responseJson);
                NFT_File uploadedFile = new NFT_File();
                uploadedFile.Name = fileInfo.Name;
                uploadedFile.Status = decodedResponse.value.pin.status;
                uploadedFile.Cid = decodedResponse.value.cid;
                return uploadedFile;
            }
            throw new NotImplementedException();
        }
    }
}
