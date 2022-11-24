namespace NFT.Storage.Net.ClientResponse
{
    /// <summary>
    /// a deal represents deals with storage providers (miners) who promise to store the file (s)
    /// </summary>
    public class ResponseDeal
    {
        public string batchRootCid { get; set; }
        public string lastChange { get; set; }
        public string miner { get; set; }
        public string network { get; set; }
        public string pieceCid { get; set; }
        public string status { get; set; }
        public string statusText { get; set; }
        public int chainDealID { get; set; }
        public string dealActivation { get; set; }
        public string dealExpiration { get; set; }
    }
}
