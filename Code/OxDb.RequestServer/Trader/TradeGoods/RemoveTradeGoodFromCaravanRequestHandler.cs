using OxDb.RequestServer.Core;
using OxDb.RequestServer.GameClientRequests.RequestHandlers;
using OxDb.SharedGame.Trader.TradeGoods.Services;
using OxDb.SharedGame.Trader.TradeGoods.WebApi;

namespace OxDb.RequestServer.Trader.TradeGoods
{
    public class RemoveTradeGoodFromCaravanRequestHandler : BaseClientUserRequestHandler<RemoveTradeGoodFromCaravanRequest>
    {

        private ITradeGoodService _tradeGoodService = null;
        protected override async Task InnerHandleMessage(WebContext context, RemoveTradeGoodFromCaravanRequest request, CancellationToken token)
        {

            context.AddResponse(await _tradeGoodService.RemoveTradeGoodFromCaravan(context, request.TradeGoodId, request.SellValue, request.UniqueId));
        }
    }
}
