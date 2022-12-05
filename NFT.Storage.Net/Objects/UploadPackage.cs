
namespace NFT.Storage.Net
{
    /// <summary>
    /// an upload package is a list of files which should be uploaded in one go. <br/>
    /// this is a lot faster than uploading many single files. UploadPipeline uses this class and tries to pack as many files as possible into one upload package
    /// </summary>
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
