using System.Net;

namespace NFT.Storage.Net.API
{
    public partial class NFT_Storage_API
    {
        public NFT_Storage_API(string apiKey, string url = "https://api.nft.storage")
        {
            if (apiKey == null)
            {
                throw new ArgumentNullException(nameof(apiKey));
            }
            _Client = new HttpClient();
            _Client.BaseAddress = new Uri(url);
            _Client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            UploadThrotteled = false;
            ServicePointManager.DefaultConnectionLimit = 40;
        }
        private HttpClient _Client;
    }
}
