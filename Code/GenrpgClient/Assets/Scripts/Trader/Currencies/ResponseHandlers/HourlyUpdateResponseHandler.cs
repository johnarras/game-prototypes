using Assets.Scripts.DynamicUI.Services;
using Assets.Scripts.Login.Messages.Core;
using Assets.Scripts.Rewards.Services;
using Assets.Scripts.Trader.ClientEvents;
using Assets.Scripts.Core;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Rewards.Services;
using Genrpg.Shared.Trader.Constants;
using Genrpg.Shared.UserEnergy.WebApi;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts.Trader.MessageHandlers.CoreCurrencies
{
    public class HourlyUpdateResponseHandler : BaseClientWebResponseHandler<HourlyUpdateResponse>
    {
        private IRewardService _rewardService = null;
        private IClientRandom _rand = null;

        protected override async Awaitable InnerProcess(HourlyUpdateResponse response, CancellationToken token)
        {
            CoreData coreData = _gs.ch.Get<CoreData>();
            coreData.NextHourlyUpdate = response.NextHourlyUpdate;
            coreData.Vars[TraderVars.PlayCount] = response.Day;
            RewardParams rp = new RewardParams();

            foreach (Reward rew in response.Rewards)
            {
                await _rewardService.GiveReward(_gs.ch, rew, new ClientRewardParams(false, true));
            }

            _dispatcher.Dispatch(new UpdateTraderHUD());
        }
    }
}


