using System;
using Xunit;

namespace NFT.Storage.Net.UnitTests
{
    public class UploadTests
    {
        [Fact]
        public void TestUploadRandomImage()
        {
            NFT_Storage_API api = new NFT_Storage_API(GlobalVar.TestApiKey);
            GUID_Image.GenerateGuidImage("Testfile1.png");
            NFT_File file = api.Upload("Testfile1.png").Result;
            string url = file.URL;
            { }
        }
    }
}