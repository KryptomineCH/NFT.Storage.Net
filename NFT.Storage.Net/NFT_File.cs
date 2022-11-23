using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFT.Storage.Net
{
    public class NFT_File
    {
        public string Name { get; set; }
        public string Cid { get; set; }
        public string URL { get
            {
                return "https://"+Cid+ ".ipfs.nftstorage.link/";
            }
        }
        public string Status { get; set; }
        public string Sha256Sum { get; private set; }
        public void CalculateChecksum()
        {
            byte[] response = new System.Net.WebClient().DownloadData(URL);
            Sha256Sum = Sha256.GetSha256Sum(response);
        }
    }
}
