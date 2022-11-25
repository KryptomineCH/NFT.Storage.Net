using System.Net.Http.Headers;
using System.Text.Json;

namespace NFT.Storage.Net.API
{
    public partial class NFT_Storage_API
    {
        internal Task UploadMultiple(UploadPackage inputFiles)
        {
            return UploadMultiple(inputFiles.Files.ToArray());
        }
        public async Task<NFT_File[]> UploadMultiple(FileInfo[] inputFiles)
        {
            long totalByteSize = 0;
            foreach (FileInfo fi in inputFiles)
            {
                if (!fi.Exists)
                {
                    throw new FileNotFoundException($"File {fi.FullName} does not exist");
                }
                totalByteSize += fi.Length;
            }
            if (totalByteSize * 0.000001 > GlobalVar.MaxUploadSize_MB)
            {
                throw new ArgumentOutOfRangeException($"Max upload size: {GlobalVar.MaxUploadSize_MB} - You are trying to upload {Math.Round(totalByteSize * 0.000001, 1)}mb!");
            }
            using (var content = new MultipartFormDataContent())
            {
                foreach (FileInfo fi in inputFiles)
                {
                    content.Add(CreateFileContent(fi));
                }
                AwaitRateLimit();
                var response = await _Client.PostAsync("upload/", content);
                response.EnsureSuccessStatusCode();
                // deserialize
                string responseJson = await response.Content.ReadAsStringAsync();
                ClientResponse.Response decodedResponse = JsonSerializer.Deserialize<ClientResponse.Response>(responseJson);
                // build return file
                List<NFT_File> uploadedFiles = new List<NFT_File>();
                for(int fileIndex = 0; fileIndex < decodedResponse.value.files.Length; fileIndex++)
                {
                    NFT_File uploadedFile = new NFT_File(bulkUpload: true);
                    uploadedFile.Name = inputFiles[fileIndex].Name;
                    uploadedFile.Cid = decodedResponse.value.cid;
                    uploadedFile.CalculateChecksum();
                    uploadedFile.LocalFile = inputFiles[fileIndex];
                    string localChecksum = Sha256.GetSha256Sum(inputFiles[fileIndex]);
                    if (localChecksum != uploadedFile.Sha256Sum)
                    {
                        throw new InvalidDataException("local checksum and checksum of uploaded File do not match!");
                    }
                    uploadedFiles.Add(uploadedFile);
                }
                return uploadedFiles.ToArray();
            }
        }
        private StreamContent CreateFileContent(FileInfo fileInfo)
        {
            var fileContent = new StreamContent(fileInfo.OpenRead());
            fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = "\"file\"",
                FileName = "\"" + fileInfo.Name + "\""
            };
            return fileContent;
        }
    }
}
