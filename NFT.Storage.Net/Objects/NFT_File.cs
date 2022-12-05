using System.Collections.Concurrent;

namespace NFT.Storage.Net
{
    /// <summary>
    /// an nft_file is a file (or folder) which has been uploaded to nft.storage
    /// </summary>
    public class NFT_File
    {
        public NFT_File(bool bulkUpload)
        {
            BulkUpload = bulkUpload;
        }
        public bool BulkUpload { get; set; }
        /// <summary>
        /// the name of the file
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// the unique identifier
        /// </summary>
        public string Cid { get; set; }
        /// <summary>
        /// the weburl by which the file can be accessed in the browser. Is generated with the unique id
        /// </summary>
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
        /// <summary>
        /// the status on nft.storage (eg pinned)
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// the checksum of the uploaded file
        /// </summary>
        /// <remarks>
        /// CalculateChecksum() needs to be called first - perhaps can be adjusted with lazy loading so that CalculateChecksum() doesngt need to be called
        /// </remarks>
        public string Sha256Sum { get; private set; }
        public FileInfo LocalFile { get; set; }
        /// <summary>
        /// accesses the files link and obtains the sha256sum
        /// </summary>
        /// <returns></returns>
        public async Task CalculateChecksum()
        {
            byte[] file = await API.DownloadClient.DownloadAsync(URL);
            Sha256Sum = Sha256.GetSha256Sum(file);
        }
    }
}
