using Assets.Scripts.ClientEvents.Entities;
using Assets.Scripts.Login.Messages.Core;
using Assets.Scripts.Trader.ClientEvents;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Rewards.Services;
using Genrpg.Shared.Trader.Constants;
using Genrpg.Shared.UserEnergy.WebApi;
using System.Threading;

namespace Assets.Scripts.Trader.MessageHandlers.CoreCurrencies
{
    public class HourlyUpdateResponseHandler : BaseClientWebResponseHandler<HourlyUpdateResponse>
    {
        private IRewardService _rewardService = null;
        private IClientRandom _rand = null;
        protected override void InnerProcess(HourlyUpdateResponse response, CancellationToken token)
        {
            CoreData coreData = _gs.ch.Get<CoreData>();
            coreData.NextHourlyUpdate = response.NextHourlyUpdate;
            coreData.Vars[TraderVars.PlayCount] = response.Day;
            RewardParams rp = new RewardParams()
            {
                SkipVisualUpdate = true,
            };

            foreach (Reward rew in response.Rewards)
            {
                _rewardService.GiveReward(_rand, _gs.ch, rew, rp);

                _dispatcher.Dispatch(new ReplaceEntityModel() { EntityTypeId = rew.EntityTypeId, EntityId = rew.EntityId });

            }

            _dispatcher.Dispatch(new UpdateTraderStatusUI());
        }
    }
}


