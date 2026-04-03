using Assets.Scripts.FloatingText.ClientEvents;
using Assets.Scripts.Login.Messages.Core;
using Genrpg.Shared.Currencies.Constants;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Rewards.Services;
using Genrpg.Shared.Trader.Caravans.PlayerData;
using Genrpg.Shared.Trader.TradeGoods.Services;
using Genrpg.Shared.Trader.TradeGoods.WebApi;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts.Trader.TradeGoods.ResponseHandlers
{
    public class RemoveTradeGoodFromCaravanResultHandler : BaseClientWebResponseHandler<RemoveTradeGoodFromCaravanResponse>
    {
        protected ITradeGoodService _tradeGoodService = null;
        private IRewardService _rewardService = null;
        protected override async Awaitable InnerProcess(RemoveTradeGoodFromCaravanResponse response, CancellationToken token)
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
        }
    }
}
