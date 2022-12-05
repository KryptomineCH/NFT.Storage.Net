using System.Collections.Concurrent;

namespace NFT.Storage.Net
{
    /// <summary>
    /// upload pipeline allows you to upload many files very fast.
    /// Just add files and in the end await WaitForUploadsToFinish()
    /// </summary>
    public class UploadPipeline
    {
        public UploadPipeline(string apiKey)
        {
            _Client = new API.NFT_Storage_API(apiKey);
        }
        private API.NFT_Storage_API _Client; 
        /// <summary>
        /// adds files to the upload queue. They will be uploaded automatically
        /// </summary>
        /// <param name="files"></param>
        /// <param name="startUploads"></param>
        public void AddFilesToUploadQueue(FileInfo[] files,bool startUploads = true)
        {
            foreach(FileInfo file in files)
            {
                AddFilesToUploadQueue(file, startUploads: startUploads, startOnlyWhenPackageIsFull: true);
            }
            if (startUploads)
            {
                StartUploads();
            }
        }
        /// <summary>
        /// add a single file to the upload queue. The queue will be processed automatically
        /// </summary>
        /// <param name="file"></param>
        /// <param name="startUploads"></param>
        /// <param name="startOnlyWhenPackageIsFull"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void AddFilesToUploadQueue(FileInfo file, bool startUploads = true, bool startOnlyWhenPackageIsFull = false)
        {
            if ( file.Length * 0.000001 > GlobalVar.MaxUploadSize_MB)
            {
                throw new ArgumentOutOfRangeException($"Max upload size: {GlobalVar.MaxUploadSize_MB} - You are trying to upload {Math.Round(file.Length * 0.000001, 1)}mb!");
            }
            bool foundSlot = false;
            if (_Client.UploadThrotteled || !startUploads || startOnlyWhenPackageIsFull)
            {
                lock (UploadPackageLock)
                {
                    if ((Upload_Package.Size + file.Length) * 0.000001 > GlobalVar.MaxUploadSize_MB || Upload_Package.Files.Count >= GlobalVar.MaxFilesPerFolder)
                    {
                        UploadPackages.Enqueue(Upload_Package.Clone());
                        Upload_Package = new UploadPackage();
                        if (!_Client.UploadThrotteled && startOnlyWhenPackageIsFull)
                        {
                            // starts a new upload task when package is full
                            StartUploads();
                        }
                    }
                    Upload_Package.Size += file.Length;
                    Upload_Package.Files.Add(file);
                }
            }
            if (!_Client.UploadThrotteled && !startOnlyWhenPackageIsFull)
            {
                StartUploads();
            }
        }
        public ConcurrentQueue<NFT_File> UploadedFiles = new ConcurrentQueue<NFT_File> ();
        object UploadPackageLock = new object ();
        private UploadPackage Upload_Package = new UploadPackage();
        private ConcurrentQueue<UploadPackage> UploadPackages = new ConcurrentQueue<UploadPackage>();
        public int RunningUploads = 0;
        private ConcurrentQueue<Task> UploadTasks = new ConcurrentQueue<Task> ();
        private ConcurrentQueue<Task> Sha256Tasks = new ConcurrentQueue<Task> ();
        /// <summary>
        /// creates a task which completes when the upload queue is fully worked through
        /// </summary>
        /// <returns></returns>
        public async Task WaitForUploadsToFinish()
        {
            while (UploadTasks.Count > 0 || Sha256Tasks.Count > 0 || UploadPackages.Count > 0 || Upload_Package.Size > 0)
            {
                Task t;
                bool taskRemoved = false;
                if (UploadTasks.TryPeek(out t))
                {
                    if (t.IsCompleted)
                    {
                        if(UploadTasks.TryDequeue(out _))
                        {
                            taskRemoved = true;
                        }
                    }
                }
                if (Sha256Tasks.TryPeek(out t))
                {
                    if (t.IsCompleted)
                    {
                        if (Sha256Tasks.TryDequeue(out _))
                        {
                            taskRemoved = true;
                        }
                    }
                }
                if (!taskRemoved)
                {
                    Task.Delay(50).Wait();
                }
            }
            { }
        }
        /// <summary>
        /// starts a task which monitors the uploat queue and automatically uploads the files. Multiple workeys may be started.
        /// </summary>
        /// <remarks>
        /// workers are disposed when the upload queue is finished
        /// </remarks>
        public void StartUploads()
        {
            UploadTasks.Enqueue(Task.Run(async () => UploadTask()));
        }
        SemaphoreSlim downloadConcurrencySemaphore = new SemaphoreSlim(GlobalVar.MaxParallelDownloads);
        /// <summary>
        /// Normally this is executed automatically if adding files (except you specify differently)
        /// </summary>
        /// <returns></returns>
        private async Task UploadTask()
        {
            if (_Client.UploadThrotteled || (Upload_Package.Size <= 0 && UploadPackages.Count <= 0))
            {
                return;
            }

            UploadPackage toUpload = null;
            if (!UploadPackages.TryDequeue(out toUpload))
            {
                lock (UploadPackageLock)
                {
                    if (Upload_Package.Size > 0)
                    {
                        toUpload = Upload_Package.Clone();
                        Upload_Package = new UploadPackage();
                    }
                }
            }
            if (toUpload != null)
            {
                try
                {
                    NFT_File[] result = _Client.UploadMultiple(toUpload.Files.ToArray(),getSha256Sums: false).Result;
                    foreach (NFT_File file in result)
                    {
                        UploadedFiles.Enqueue(file);
                        var t = Task.Run(async () =>
                        {
                            Thread.CurrentThread.Name = "sha256PipelineDownloader";
                            await downloadConcurrencySemaphore.WaitAsync();
                            try
                            {
                                await file.CalculateChecksum();
                            }
                            catch (Exception ex)
                            {
                                { }
                            }
                            finally
                            {
                                downloadConcurrencySemaphore.Release();
                            }
                        });
                        Sha256Tasks.Enqueue(t);
                    }
                }
                catch (Exception ex) when (!toUpload.Error)
                {
                    // try one more time if error
                    toUpload.Error = true;
                    UploadPackages.Enqueue(toUpload);
                }
                StartUploads();
            }
        }
    }
}
