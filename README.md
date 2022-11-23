# NFT.Storage.Net
c# library to easily upload files to NFT.Storage.

#### loads files directly into your account and returns a readable .net object
![image](https://user-images.githubusercontent.com/117320700/203649841-7abd43d6-4c4a-44be-abdc-ddf5ae5ecc0e.png)
## Usage:
```
public void TestUploadRandomImage()
{   
    // can upload local files from disk right now
    GUID_Image.GenerateGuidImage("Testfile1.png");
    // create api
    NFT_Storage_API api = new NFT_Storage_API(GlobalVar.TestApiKey);
    
    
    NFT_File file = api.Upload("Testfile1.png").Result;
    string url = file.URL;
    string checksum = file.Sha256Sum;
}
```
-> returns a simplified c# object file
![image](https://user-images.githubusercontent.com/117320700/203665613-a43fc570-b51b-497a-a859-eb62c7be4e30.png)

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
