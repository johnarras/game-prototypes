using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crypto.Entities
{
    public class EthereumTransactionList
    {
        public string WalletAddress { get; set; }
        public string Message { get; set; }
        public List<EthereumTransaction> result { get; set; } = new List<EthereumTransaction>();
    }
}


