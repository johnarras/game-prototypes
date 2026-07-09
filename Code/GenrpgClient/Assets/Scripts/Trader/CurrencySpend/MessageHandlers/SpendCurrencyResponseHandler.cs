using Assets.Scripts.FloatingText.ClientEvents;
using Assets.Scripts.Login.Messages.Core;
using Assets.Scripts.Trader.ClientEvents;
using OxDb.SharedCore.Rewards.Entities;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Rewards.Services;
using OxDb.SharedGame.Trader.CurrencySpend.Entities;
using OxDb.SharedGame.Trader.CurrencySpend.WebApi;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.Trader.CurrencySpend.MessageHandlers
{
    public class SpendCurrencyResponseHandler : BaseClientWebResponseHandler<SpendCurrencyResponse>
    {
        private IRewardService _rewardService = null;
        protected override async ValueTask InnerProcess(SpendCurrencyResponse response, CancellationToken token)
        {
            if (response.State != ESpendCurrencyCheckState.Success)
            {
                _dispatcher.Dispatch(new ShowFloatingText(StrUtils.SplitOnCapitalLetters(response.State.ToString()), EFloatingTextArt.Error));
                return;
            }

            RewardParams args = new RewardParams()
            {
                ExtraRewardArgs = response.ExtraRewardArgs,
            };

            await _rewardService.GiveRewards(_gs.ch, response.Rewards, args);
            _dispatcher.Dispatch(new UpdateTraderHUD());
            _dispatcher.Dispatch(response);
        }
    }
}
