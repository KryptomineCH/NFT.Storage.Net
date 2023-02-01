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
        public static byte[] DownloadSync(Uri url, IProgress<double> progress = null)
        {
            Task<byte[]> data = Task.Run(() => DownloadClient.DownloadAsync(url, progress));
            data.Wait();
            return data.Result;
        }
        public static byte[] DownloadSync(string url, IProgress<double> progress = null)
        {
            Task<byte[]> data = Task.Run(() => DownloadClient.DownloadAsync(url, progress));
            data.Wait();
            return data.Result;
        }
        public static async Task<byte[]> DownloadAsync(Uri url, IProgress<double> progress = null)
        {
            return await DownloadAsync(url.ToString(),progress);
        }
        public static async Task<byte[]> DownloadAsync(string url, IProgress<double> progress = null)
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
                long contentLength = response.Content.Headers.ContentLength.GetValueOrDefault();
                if (contentLength <= 0 || progress == null)
                {
                    // If content length is unknown, read the entire response as a stream.
                    byte[] file = await response.Content.ReadAsByteArrayAsync();
                    return file;
                }

                using (var stream = await response.Content.ReadAsStreamAsync())
                {
                    int bufferSize = 1024 * 4;
                    var buffer = new byte[bufferSize];
                    int bytesRead;
                    long totalBytesRead = 0;

                    while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        totalBytesRead += bytesRead;
                        progress?.Report((double)totalBytesRead / contentLength);
                    }

                    return buffer.Take((int)totalBytesRead).ToArray();
                }
            }
            throw new HttpRequestException("File Download failed!");
        }
    }
}
