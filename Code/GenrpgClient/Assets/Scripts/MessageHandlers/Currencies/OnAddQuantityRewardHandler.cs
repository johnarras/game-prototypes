
using Genrpg.Shared.Rewards.Messages;
using Genrpg.Shared.Rewards.Services;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts.MessageHandlers.Currency
{
    public class OnAddQuantityRewardHandler : BaseClientMapMessageHandler<OnAddQuantityReward>
    {
        protected IRewardService _rewardService = null;
        protected override async Awaitable InnerProcess(OnAddQuantityReward msg, CancellationToken token)
        {
            if (msg.CharId != _gs.ch.Id)
            {
                return;
            }

            await _rewardService.GiveReward(_gs.ch, msg.EntityTypeId, msg.EntityId, msg.Quantity, null, null);
            _dispatcher.Dispatch(msg);
        }
    }
}


