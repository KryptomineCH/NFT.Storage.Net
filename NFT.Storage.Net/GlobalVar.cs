namespace NFT.Storage.Net
{
    /// <summary>
    /// Contains several library wide variables, eg settings
    /// </summary>
    internal static class GlobalVar
    {
        public const int RateLimit_MaxRequests = 30;
        public static TimeSpan RateLimit_BacklogTimeSpan = TimeSpan.FromSeconds(10);
        public const double MaxUploadSize_MB = 100.0;
        public const int MaxFilesPerFolder = 10000;
        public const int MaxParallelDownloads = 90;
    }
}
