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


