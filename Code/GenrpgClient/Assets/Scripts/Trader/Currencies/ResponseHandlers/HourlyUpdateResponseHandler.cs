using Assets.Scripts.Login.Messages.Core;
using Assets.Scripts.Rewards.Services;
using Assets.Scripts.Trader.ClientEvents;
using OxDb.SharedCore.Rewards.Entities;
using OxDb.SharedGame.Core.PlayerData;
using OxDb.SharedGame.Rewards.Constants;
using OxDb.SharedGame.Rewards.Services;
using OxDb.SharedGame.Trader.Constants;
using OxDb.SharedGame.UserEnergy.WebApi;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Trader.MessageHandlers.CoreCurrencies
{
    public class HourlyUpdateResponseHandler : BaseClientWebResponseHandler<HourlyUpdateResponse>
    {
        private IRewardService _rewardService = null;

        protected override async Awaitable InnerProcess(HourlyUpdateResponse response, CancellationToken token)
        {
            CoreData coreData = _gs.ch.Get<CoreData>();
            coreData.NextHourlyUpdate = response.NextHourlyUpdate;
            coreData.Vars[TraderVars.PlayCount] = response.Day;
            RewardParams rp = new RewardParams();

            foreach (Reward rew in response.Rewards)
            {
                await _rewardService.GiveReward(_gs.ch, rew, RewardSources.HourlyUpdate, new ClientRewardParams(false, true));
            }

            _dispatcher.Dispatch(new UpdateTraderHUD());
            await Task.CompletedTask;
        }
    }
}


