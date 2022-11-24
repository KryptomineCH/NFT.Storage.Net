namespace NFT.Storage.Net.ClientResponse
{
    /// <summary>
    /// the deserialized response of nft.storage api
    /// </summary>
    public partial class Response
    {
        /// <summary>
        /// Response status
        /// </summary>
        public bool ok { get; set; }
        /// <summary>
        /// details of the file such as size, creation date, pin status and cid
        /// </summary>
        public ResponseValue @value { get; set; }
        /// <summary>
        /// supposedly information about deals with storage providers when pulling data from api
        /// </summary>
        public ResponseDeal[] deals { get; set; }
        public override string ToString()
        {
            return ok.ToString();
        }
    } 
}
