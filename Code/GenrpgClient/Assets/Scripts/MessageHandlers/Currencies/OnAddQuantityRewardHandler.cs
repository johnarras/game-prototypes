
using OxDb.SharedGame.Rewards.Messages;
using OxDb.SharedGame.Rewards.Services;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.MessageHandlers.Currency
{
    public class OnAddQuantityRewardHandler : BaseClientMapMessageHandler<OnAddQuantityReward>
    {
        protected IRewardService _rewardService = null;
        protected override async ValueTask InnerProcess(OnAddQuantityReward msg, CancellationToken token)
        {
            if (msg.CharId != _gs.ch.Id)
            {
                return;
            }

            await _rewardService.GiveReward(_gs.ch, msg.EntityTypeId, msg.EntityId, msg.Quantity, msg.RewardSourceId, null, 0, null);
            _dispatcher.Dispatch(msg);
        }
    }
}


