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
-> returns a simplified c# object file
![image](https://user-images.githubusercontent.com/117320700/203652251-68b6fdd9-ed73-4e2d-82d4-c2542629ecd6.png)


## supported filetypes
- stream data
- locally stored files
- files up to 100 mb

## features
- upload image to nft.storage
- validate and show sha256sum
- async / multithreading
- unit tests
- api key

## loads files directly into your account and returns a readable .net object
![image](https://user-images.githubusercontent.com/117320700/203649841-7abd43d6-4c4a-44be-abdc-ddf5ae5ecc0e.png)
