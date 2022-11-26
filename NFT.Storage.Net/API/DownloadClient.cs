using System.Net;

namespace NFT.Storage.Net.API
{
    public static class DownloadClient
    {
        static DownloadClient()
        {
            _Client = new HttpClient();

            var handler = new HttpClientHandler();
            handler.ClientCertificateOptions = ClientCertificateOption.Manual;
            handler.ServerCertificateCustomValidationCallback =
                (httpRequestMessage, cert, cetChain, policyErrors) =>
                {
                    return true;
                };
            handler.MaxConnectionsPerServer = GlobalVar.MaxParallelDownloads;

            _Client = new HttpClient(handler);
            _Client.DefaultRequestHeaders.ConnectionClose = false;
        }
        private static HttpClient _Client;
        public static async Task<byte[]> Download(string url)
        {
            for (int retries = 0; retries < 3; retries++)
            {
                HttpResponseMessage response = await _Client.GetAsync(url);
                try
                {
                    response.EnsureSuccessStatusCode();
                }
                catch (Exception ex) when (false)
                {
                    { }
                    await Task.Delay(50);
                    continue;
                }
                catch (Exception ex) when (retries < 2)
                {
                    { }
                    await Task.Delay(50);
                    continue;
                }
                System.Net.Http.HttpContent content = response.Content;
                byte[] file = await content.ReadAsByteArrayAsync();
                return file;
            }
            throw new HttpRequestException("File Download failed!");
        }
    }
}
