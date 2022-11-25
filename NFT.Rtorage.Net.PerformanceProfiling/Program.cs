using NFT.Storage.Net.UnitTests;
using System.Net;

ServicePointManager.DefaultConnectionLimit = 40;
UploadTests unitTests = new UploadTests();
unitTests.TestUploadPipeline_plenty();