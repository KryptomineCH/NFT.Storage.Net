# NFT.Storage.Net
c# library to easily upload files to NFT.Storage.
## Usage:
```
public class UploadTests
{
    [Fact]
    public void TestUploadRandomImage()
    {   
        // can upload local files from disk right now
        GUID_Image.GenerateGuidImage("Testfile1.png");
        // create api
        NFT_Storage_API api = new NFT_Storage_API(GlobalVar.TestApiKey);
        
        
        NFT_File file = api.Upload("Testfile1.png").Result;
        string url = file.URL;
        { }
    }
}
```

## supports
- stream data
- locally stored files
- files up to 100 mb
- image files verified for now
- async / multithreading

![image](https://user-images.githubusercontent.com/117320700/203649841-7abd43d6-4c4a-44be-abdc-ddf5ae5ecc0e.png)
