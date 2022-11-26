using System.Collections.Concurrent;

namespace NFT.Storage.Net
{
    public class NFT_File
    {
        public NFT_File(bool bulkUpload)
        {
            BulkUpload = bulkUpload;
        }
        public bool BulkUpload { get; set; }
        public string Name { get; set; }
        public string Cid { get; set; }
        public string URL { get
            {
                if (!BulkUpload)
                {
                    return $"https://{Cid}.ipfs.nftstorage.link/";
                }
                else
                {
                    return $"https://{Cid}.ipfs.nftstorage.link/{Name}";
                }
            }
        }
        public string Status { get; set; }
        public string Sha256Sum { get; private set; }
        public FileInfo LocalFile { get; set; }
        public async Task CalculateChecksum()
        {
            byte[] file = await API.DownloadClient.DownloadAsync(URL);
            Sha256Sum = Sha256.GetSha256Sum(file);
        }
    }
}
