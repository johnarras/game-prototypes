using OxDb.SharedCore.Website.Interfaces;

namespace OxDb.SharedGame.Trader.TradeGoods.WebApi
{
    public class RemoveTradeGoodFromCaravanRequest : IClientUserRequest
    {
        public long TradeGoodId { get; set; }
        public long SellValue { get; set; }
        public long UniqueId { get; set; }
    }
}
