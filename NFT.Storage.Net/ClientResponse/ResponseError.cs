using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFT.Storage.Net.ClientResponse
{
    public class ResponseError
    {
        public bool error { get; set; }
        public string name { get; set; }
        public string message { get; set; }
    }
}
