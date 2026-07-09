using Assets.Scripts.FloatingText.ClientEvents;
using Assets.Scripts.Login.Messages.Core;
using OxDb.SharedGame.Trader.TradeGoods.Services;
using OxDb.SharedGame.Trader.TradeGoods.WebApi;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.Trader.TradeGoods.ResponseHandlers
{
    public class RemoveTradeGoodFromCaravanResultHandler : BaseClientWebResponseHandler<RemoveTradeGoodFromCaravanResponse>
    {
        protected ITradeGoodService _tradeGoodService = null;
        protected override async ValueTask InnerProcess(RemoveTradeGoodFromCaravanResponse response, CancellationToken token)
        {

            if (!response.Success)
            {
                _dispatcher.Dispatch(new ShowFloatingText(response.ErrorMessage, EFloatingTextArt.Error));
            }
            else
            {
                await _tradeGoodService.RemoveTradeGoodFromCaravan(_gs.ch, response.TradeGoodId, response.SellValue, response.UniqueId);
                _dispatcher.Dispatch(response);
            }
            await Task.CompletedTask;
        }
    }
}
