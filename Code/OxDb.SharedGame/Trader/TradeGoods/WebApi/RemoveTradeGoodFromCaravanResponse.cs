using OxDb.SharedCore.Website.Responses.Interfaces;
using OxDb.SharedGame.Trader.Caravans.Entities;

namespace OxDb.SharedGame.Trader.TradeGoods.WebApi
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


