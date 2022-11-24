using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Xunit;

namespace NFT.Storage.Net.UnitTests
{
    public class UploadTests
    {
        [Fact]
        public void TestUploadRandomImage()
        {
            API.NFT_Storage_API api = new API.NFT_Storage_API(GlobalVar.TestApiKey);
            GUID_Image.GenerateGuidImage("/temp/TestUploadRandomImage.png");
            NFT_File file = api.Upload("/temp/TestUploadRandomImage.png").Result;
            string url = file.URL;
            { }
        }
        [Fact]
        public void TestBulkUpload()
        {
            API.NFT_Storage_API api = new API.NFT_Storage_API(GlobalVar.TestApiKey);
            int imageCount = 50;
            List<FileInfo> files = new List<FileInfo>();
            for (int i = 0; i < imageCount; i++)
            {
                GUID_Image.GenerateGuidImage($"/temp/TestBulkUpload{i}.png");
                files.Add(new FileInfo($"/temp/TestBulkUpload{i}.png"));
            }
            
            NFT_File[] nft_files = api.UploadMultipe(files.ToArray()).Result;
            { }
        }
        [Fact]
        public void TestUploadPipeline_plenty()
        {
            UploadPipeline pipeline = new UploadPipeline(GlobalVar.TestApiKey);
            int imageCount = 50000;
            List<FileInfo> files = new List<FileInfo>();
            for (int i = 0; i < imageCount; i++)
            {
                GUID_Image.GenerateGuidImage($"/temp/TestUploadPipeline_plenty{i}.png");
                files.Add(new FileInfo($"/temp/TestUploadPipeline_plenty{i}.png"));
            }
            pipeline.AddFilesToUploadQueue(files.ToArray());
            pipeline.WaitForUploadsToFinish().Wait();
            NFT_File[] nft_files = pipeline.UploadedFiles.ToArray();
            { }
        }
        [Fact]
        public void TestUploadPipeline_large()
        {
            UploadPipeline pipeline = new UploadPipeline(GlobalVar.TestApiKey);
            int imageCount = 1000;
            List<FileInfo> files = new List<FileInfo>();
            Bitmap LargeImage = GUID_Image.LargeImage();
            Bitmap temp;
            for (int i = 0; i < imageCount; i++)
            {
                if (File.Exists($"/temp/TestUploadPipeline_large{i}.png"))
                {
                    File.Delete($"/temp/TestUploadPipeline_large{i}.png");
                }
                temp = GUID_Image.BrandImage(LargeImage);
                temp.Save($"/temp/TestUploadPipeline_large{i}.png");
                files.Add(new FileInfo($"/temp/TestUploadPipeline_large{i}.png"));
            }
            pipeline.AddFilesToUploadQueue(files.ToArray());
            pipeline.WaitForUploadsToFinish().Wait();
            NFT_File[] nft_files = pipeline.UploadedFiles.ToArray();
            { }
        }
    }
}