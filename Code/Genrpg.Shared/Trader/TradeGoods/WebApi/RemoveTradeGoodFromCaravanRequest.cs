using Genrpg.Shared.Website.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Trader.TradeGoods.WebApi
{
    public class RemoveTradeGoodFromCaravanRequest : IClientUserRequest
    {
        public long TradeGoodId { get; set; }
        public long SellValue { get; set; }
        public long UniqueId { get; set; }
    }
}
