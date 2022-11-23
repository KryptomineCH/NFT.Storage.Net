

using System.IO;

namespace NFT.Storage.Net.UnitTests
{
    internal static class GlobalVar
    {
        static GlobalVar()
        {
            TestApiKey = File.ReadAllText("ApiKey.txt");
        }
        /// <summary>
        /// note tó update the key in ApiKey.txt
        /// </summary>
        internal static string TestApiKey { get; set; }
    }
}
