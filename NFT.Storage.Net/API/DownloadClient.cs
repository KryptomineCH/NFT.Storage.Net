using System;
using System.Diagnostics;
using System.Net;
using System.Threading;

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
        public static byte[] DownloadSync(Uri url)
        {
            Task<byte[]> data = Task.Run(() => DownloadClient.DownloadAsync(url));
            data.Wait();
            return data.Result;
        }
        public static byte[] DownloadSync(string url)
        {
            Task<byte[]> data = Task.Run(() => DownloadClient.DownloadAsync(url));
            data.Wait();
            return data.Result;
        }
        public static async Task<byte[]> DownloadAsync(Uri url, IProgress<float> progress = null, CancellationToken cancellationToken = default)
        {
            return await DownloadAsync(url.ToString(), progress,cancellationToken);
        }
        public static async Task<byte[]> DownloadAsync(string url, IProgress<float> progress = null, CancellationToken cancellationToken = default)
        {
            for (int retries = 0; retries < 3; retries++)
            {
                try
                {
                    return await _Client.DownloadToMemoryAsync(url, progress, cancellationToken);
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
            }
            throw new HttpRequestException("File Download failed!");
        }
    }
}
