using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFT.Storage.Net
{
    public class API
    {
        public API(string apiKey, string url = "https://api.nft.storage")
        {
            _Client = new HttpClient();
            _Client.BaseAddress = new Uri(url);
            _Client.DefaultRequestHeaders.Add("api-key", apiKey);
            _NFTClient = new Client.NFTStorageClient(_Client);
        }
        private HttpClient _Client;
        private Client.NFTStorageClient _NFTClient;
        public NFT_File Upload(string localPath)
        {
            using (FileStream fs = File.OpenRead(localPath))
            {
                var result = _NFTClient.StoreAsync(fs).Result;
                { }
            }
            throw new NotImplementedException();
        }
    }
}
