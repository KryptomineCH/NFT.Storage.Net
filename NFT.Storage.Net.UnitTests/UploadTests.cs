using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace NFT.Storage.Net.UnitTests
{
    public class UploadTests
    {
        [Fact]
        public void TestUploadRandomImage()
        {
            DirectoryInfo testDir = TestHelpers.CleanTestDir("TestUploadRandomImage");
            FileInfo testFile = new FileInfo(Path.Combine(testDir.FullName, $"TestUploadRandomImage"));
            API.NFT_Storage_API api = new API.NFT_Storage_API(GlobalVar.TestApiKey);
            GUID_Image.GenerateGuidImage(testFile.FullName);
            NFT_File file = api.Upload(testFile).Result;
            string url = file.URL;
        }
        [Fact]
        public void TestBulkUpload()
        {
            DirectoryInfo testDir = TestHelpers.CleanTestDir("TestBulkUpload");
            API.NFT_Storage_API api = new API.NFT_Storage_API(GlobalVar.TestApiKey);
            int imageCount = 50;
            List<FileInfo> files = new List<FileInfo>();
            for (int i = 0; i < imageCount; i++)
            {
                FileInfo testFile = new FileInfo(Path.Combine(testDir.FullName, $"TestBulkUpload{i}.png"));
                GUID_Image.GenerateGuidImage(testFile.FullName);
                files.Add(testFile);
            }
            
            NFT_File[] nft_files = api.UploadMultiple(files.ToArray()).Result;
            if (nft_files.Length != imageCount)
            {
                throw new InvalidDataException($"final images: {nft_files.Length}; should be: {imageCount}!");
            }
        }
        [Fact]
        public void TestUploadPipeline_plenty()
        {
            DirectoryInfo testDir = TestHelpers.CleanTestDir("TestUploadPipeline_plenty");
            UploadPipeline pipeline = new UploadPipeline(GlobalVar.TestApiKey);
            int imageCount = 25000;
            ConcurrentBag<FileInfo> files = new ConcurrentBag<FileInfo>();
            List<Task> tasks = new List<Task>();
            for (int i = 0; i < imageCount; i++)
            {
                FileInfo testFile = new FileInfo(Path.Combine(testDir.FullName, $"TestUploadPipeline_plenty{i}.png"));
                tasks.Add(Task.Run(async () =>
                {
                    GUID_Image.GenerateGuidImage(testFile.FullName);
                    files.Add(testFile);
                }));
            }
            Task.WaitAll(tasks.ToArray());
            pipeline.AddFilesToUploadQueue(files.ToArray());
            pipeline.WaitForUploadsToFinish().Wait();
            NFT_File[] nft_files = pipeline.UploadedFiles.ToArray();
            if (nft_files.Length != imageCount)
            {
                throw new InvalidDataException($"final images: {nft_files.Length}; should be: {imageCount}!");
            }
            for (int i = 0; i < imageCount; i++)
            {
                if (nft_files[i].Sha256Sum == null || nft_files[i].Sha256Sum == "")
                {
                    throw new InvalidDataException("ShaSum is empty!");
                }
            }
        }
        [Fact]
        public void TestUploadPipeline_large()
        {
            DirectoryInfo testDir = TestHelpers.CleanTestDir("TestUploadPipeline_large");
            UploadPipeline pipeline = new UploadPipeline(GlobalVar.TestApiKey);
            int imageCount = 1000;
            List<FileInfo> files = new List<FileInfo>();
            Bitmap LargeImage = GUID_Image.LargeImage();
            Bitmap temp;
            for (int i = 0; i < imageCount; i++)
            {
                FileInfo testFile = new FileInfo(Path.Combine(testDir.FullName, $"TestUploadPipeline_large{i}.png"));
                temp = GUID_Image.BrandImage(LargeImage);
                temp.Save(testFile.FullName);
                files.Add(testFile);
            }
            pipeline.AddFilesToUploadQueue(files.ToArray());
            pipeline.WaitForUploadsToFinish().Wait();
            NFT_File[] nft_files = pipeline.UploadedFiles.ToArray();
            if (nft_files.Length != imageCount)
            {
                throw new InvalidDataException($"final images: {nft_files.Length}; should be: {imageCount}!");
            }
        }
    }
}