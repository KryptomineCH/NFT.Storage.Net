namespace NFT.Storage.Net.API
{
    public static class HttpClientExtensions
    { 
        public static async Task<byte[]> DownloadToMemoryAsync(this HttpClient client, string requestUri, IProgress<float> progress = null, CancellationToken cancellationToken = default)
        {
            // Get the http headers first to examine the content length
            using (var response = await client.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead))
            {
                var contentLength = response.Content.Headers.ContentLength;

                using (var download = await response.Content.ReadAsStreamAsync())
                {
                    if (progress != null)
                    {
                        var relativeProgress = new Progress<long>(totalBytes => progress.Report((float)totalBytes / contentLength.Value));
                        return await download.ToArrayAsync(contentLength, relativeProgress, cancellationToken);
                    }
                    else
                    {
                        return await download.ToArrayAsync(contentLength, null, cancellationToken);
                    }
                }
            }
        }
    }
    public static class StreamExtensions
    {
        /// <summary>
        /// Converts stream to byte array.
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Stream data as array</returns>
        /// <returns>Binary data from stream in an array</returns>
        public static async Task<byte[]> ToArrayAsync(this Stream stream, long? contentLength = null, IProgress<long> progress = null, CancellationToken cancellationToken = default)
        {
            if (!stream.CanRead)
            {
                throw new AccessViolationException("Stream cannot be read");
            }
            if (stream.CanSeek || contentLength != null)
            {
                return await ToArrayAsyncDirect(stream, contentLength, progress, cancellationToken);
            }
            else
            {
                return await ToArrayAsyncGeneral(stream, progress, cancellationToken);
            }
        }

        /// <summary>
        /// Converts stream to byte array through MemoryStream. This doubles allocations compared to ToArrayAsyncDirect.
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        private static async Task<byte[]> ToArrayAsyncGeneral(Stream stream, IProgress<long> progress = null, CancellationToken cancellationToken = default)
        {
            using MemoryStream memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream, cancellationToken);
            return memoryStream.ToArray();
        }

        /// <summary>
        /// Converts stream to byte array without unnecessary allocations.
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Stream data as array</returns>
        /// <exception cref="ArgumentException">Thrown if stream is not providing correct Length</exception>
        private static async Task<byte[]> ToArrayAsyncDirect(Stream stream, long? contentLength, IProgress<long> progress = null, CancellationToken cancellationToken = default)
        {

            if (contentLength == null &&  stream.Position > 0)
            {
                throw new ArgumentException("Stream is not at the start!");
            }


            var array = new byte[contentLength ?? stream.Length];
            var buffer = new byte[81920]; // ~80 KB

            long totalBytesRead = 0;
            int bytesRead;

            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false)) != 0)
            {
                Array.Copy(
                    sourceArray: buffer, 
                    sourceIndex: 0, 
                    destinationArray: array, 
                    destinationIndex: totalBytesRead, 
                    length:  bytesRead);
                totalBytesRead += bytesRead;
                if (progress != null)
                {
                    progress?.Report(totalBytesRead);
                }
            }
            return array;
        }
    }
}
