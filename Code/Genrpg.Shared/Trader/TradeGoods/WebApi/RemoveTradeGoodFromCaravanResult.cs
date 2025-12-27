using MessagePack;
using Genrpg.Shared.Trader.Caravans.Entities;
using Genrpg.Shared.Website.Interfaces;

namespace Genrpg.Shared.Trader.TradeGoods.WebApi
{
    public class RemoveTradeGoodFromCaravanResult : IWebResponse
    {
        public long TradeGoodId { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public CaravanTravelInfo Travel { get; set; }
        public long SellValue { get; set; }
    }
}


