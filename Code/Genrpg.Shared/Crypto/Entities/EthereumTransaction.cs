using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crypto.Entities
{
    public class EthereumTransaction
    {
        public string hash { get; set; }
        public string from { get; set; }
        public string to { get; set; }
        public int isError { get; set; }
    }
}


