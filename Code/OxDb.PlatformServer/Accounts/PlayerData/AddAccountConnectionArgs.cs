using System;
using System.Collections.Generic;
using System.Text;

namespace OxDb.PlatformServer.Accounts.PlayerData
{
    public class AddAccountConnectionArgs
    {
        public string AccountId { get; set; }
        public string ReferrerDisplayName { get; set; }
        public long AccountProductId { get; set; }
        public bool JustAddedNewProduct { get; set; }

        public AddAccountConnectionArgs(string accountId, string referrerDisplayName, long accountProductId, bool justAddedNewProduct)
        {
            AccountId = accountId;
            ReferrerDisplayName = referrerDisplayName;
            AccountProductId = accountProductId;
            JustAddedNewProduct = justAddedNewProduct;
        }
    }
}
