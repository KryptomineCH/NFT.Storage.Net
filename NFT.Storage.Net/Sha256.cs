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
        public static string GetSha256Sum(byte[] input)
        {
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
