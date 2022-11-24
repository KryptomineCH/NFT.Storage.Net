using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFT.Storage.Net
{
    internal class UploadPackage
    { 
        public UploadPackage()
        {
            Size = 0;
            Files = new List<FileInfo>();
            Error = false;
        }
        public bool Error { get; set; }
        public List<FileInfo> Files { get; set; }
        public long Size { get; set; }
        public UploadPackage Clone()
        {
            UploadPackage result = new UploadPackage();
            result.Size = Size;
            result.Files = Files;
            result.Error = Error;
            return result;
        }
    }
}
