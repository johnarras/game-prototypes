using Assets.Scripts.ClientEvents.Entities;
using Assets.Scripts.Login.Messages.Core;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Rewards.Services;
using Genrpg.Shared.UserEnergy.WebApi;
using System.Threading;

namespace Assets.Scripts.Trader.MessageHandlers.CoreCurrencies
{
    public class UpdateCoreCurrenciesResponseHandler : BaseClientWebResponseHandler<UpdateCoreCurrenciesResponse>
    {
        private IRewardService _rewardService = null;
        private IClientRandom _rand = null;
        protected override void InnerProcess(UpdateCoreCurrenciesResponse response, CancellationToken token)
        {
            CoreUserData userData = _gs.ch.Get<CoreUserData>();
            userData.NextHourlyUpdate = response.NextHourlyUpdate;

            RewardParams rp = new RewardParams()
            {
                SkipVisualUpdate = true,
            };

            foreach (Reward rew in response.Rewards)
            {
                _rewardService.GiveReward(_rand, _gs.ch, rew, rp);

                _dispatcher.Dispatch(new ReplaceEntityModel() { EntityTypeId = rew.EntityTypeId, EntityId = rew.EntityId });

            }
        }
    }
}


