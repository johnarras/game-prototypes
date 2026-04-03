using Genrpg.RequestServer.ClientUserRequests.RequestHandlers;
using Genrpg.RequestServer.Core;
using Genrpg.Shared.Trader.TradeGoods.Services;
using Genrpg.Shared.Trader.TradeGoods.WebApi;

namespace Genrpg.RequestServer.Trader.TradeGoods
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
