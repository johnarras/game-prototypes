using Genrpg.Shared.Inventory.Entities;
using Genrpg.Shared.Trader.Caravans.Entities;
using Genrpg.Shared.Website.Interfaces;

namespace Genrpg.Shared.Trader.TradeGoods.WebApi
{
    public class RemoveTradeGoodFromCaravanResponse : IWebResponse
    {
        public long TradeGoodId { get; set; }
        public long UniqueId { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public CaravanTravelInfo Travel { get; set; }
        public long SellValue { get; set; }
    }
}


