using ClientEvents;
using Genrpg.Shared.Entities.Services;
using Genrpg.Shared.Loot.Messages;
using Genrpg.Shared.Rewards.Services;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts.MessageHandlers.Rewards
{
    public class SendRewardsHandler : BaseClientMapMessageHandler<SendRewards>
    {
        protected IRewardService _rewardService = null;
        protected override async Awaitable InnerProcess(SendRewards msg, CancellationToken token)
        {
            await _rewardService.GiveRewards(_gs.ch, msg.Rewards, null);

            if (msg.ShowPopup)
            {
                _dispatcher.Dispatch(new ShowLootEvent() { Rewards = msg.Rewards });
            }
        }
    }
}


