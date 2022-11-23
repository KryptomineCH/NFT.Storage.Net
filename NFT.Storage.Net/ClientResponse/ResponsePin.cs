using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFT.Storage.Net.ClientResponse
{
    /// <summary>
    /// represents the pin status of a file on nft.storage
    /// </summary>
    public class ResponsePin
    {
        /// <summary>
        /// the unique identifier
        /// </summary>
        /// <remarks>
        /// Of the file or of the pin?
        /// </remarks>
        public string cid { get; set; }
        /// <summary>
        /// when was the (file?) created?
        /// </summary>
        public string created { get; set; }
        /// <summary>
        /// the size of the (file?)
        /// </summary>
        public int size { get; set; }
        /// <summary>
        /// The pin status of the file
        /// </summary>
        public string status { get; set; }
    }
}
