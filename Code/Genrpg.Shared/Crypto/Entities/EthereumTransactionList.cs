using System.Collections.Generic;

namespace Genrpg.Shared.Crypto.Entities
{
    public class EthereumTransactionList
    {
        public string WalletAddress { get; set; }
        public string Message { get; set; }
        public List<EthereumTransaction> result { get; set; } = new List<EthereumTransaction>();
    }
}


