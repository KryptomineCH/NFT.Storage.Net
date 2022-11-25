using System.IO;

namespace NFT.Storage.Net.UnitTests
{
    internal class TestHelpers
    {
        internal static DirectoryInfo CleanTestDir(string testName)
        {
            DirectoryInfo testDir = new DirectoryInfo(Path.Combine("temp",testName));
            if (testDir.Exists)
                testDir.Delete(recursive: true);
            testDir.Create();
            return testDir;
        }
    }
}
