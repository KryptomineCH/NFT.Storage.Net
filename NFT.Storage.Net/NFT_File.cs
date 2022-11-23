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
                return Cid+ ".ipfs.nftstorage.link";
            }
        }
        public string Status { get; set; }
        public string Sha256Sum { get { throw new NotImplementedException(); } set { } }
    }
}
