using System.Security.Cryptography;
using System.Text;

namespace NFT.Storage.Net
{
    public class Sha256
    {
        public static string GetSha256Sum(string  path)
        {
            return GetSha256Sum(new FileInfo(path));
        }
        public static string GetSha256Sum(FileInfo fileInfo)
        {
            return GetSha256Sum(File.ReadAllBytes(fileInfo.FullName));
        }
        public static string GetSha256Sum(Uri link)
        {

            // download files in parallel
            //Task<byte[]> downloadedFile = API.DownloadClient.DownloadAsync(link.ToString());
            Task<byte[]> downloadedFile = Task.Run(() => API.DownloadClient.DownloadAsync(link.ToString()));
            downloadedFile.Wait();
            return GetSha256Sum(downloadedFile.Result);
        }
        public static string ValidateChecksums(string[] links)
        {
            List<Uri> adresses = new List<Uri>();
            foreach (string link in links)
            {
                adresses.Add(new Uri(link));
            }
            return ValidateChecksums(adresses.ToArray());
        }

            public static string ValidateChecksums(Uri[] links)
        {
            string[] checksums = new string[links.Length];
            // download files in parallel
            Task<byte[]>[] downloadTasks = new Task<byte[]>[links.Length];
            for (int i = 0; i < links.Length; i++)
            {
                downloadTasks[i] = API.DownloadClient.DownloadAsync(links[i].ToString());
            }
            Task.WaitAll(downloadTasks);
            // generate checksums
            for (int i = 0; i < links.Length; i++)
            {
                byte[] file = downloadTasks[i].Result;
                checksums[i] = Sha256.GetSha256Sum(file);
            }
            // make sure all checksums are equal
            for (int i = 1; i < checksums.Length; i++)
            {
                if (checksums[0] != checksums[i])
                {
                    throw new InvalidDataException($"Checksums of the following files do not match! {Environment.NewLine}" +
                        $"{links[0]} {Environment.NewLine} " +
                        $"{links[i]}");
                }
            }
            return checksums[0];
        }
        public static string GetSha256Sum(byte[] input)
        {
            if (input == null || input.Length == 0)
            {
                return "";
            }
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(input);

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
