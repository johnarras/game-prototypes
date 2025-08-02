using Genrpg.RequestServer.Core;
using Genrpg.Shared.DataStores.Categories.PlayerData.ParentChild;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Rewards.Entities;

namespace Genrpg.RequestServer.Rewards.RewardHelpers.Core
{
    /// <summary>
    /// Give out web rewards for things that we want to load incrementally rather than huge items.
    /// </summary>
    /// <typeparam name="TParent"></typeparam>
    /// <typeparam name="TChild"></typeparam>
    public abstract class BaseAsyncOwnerQuantityRewardHelper<TParent, TChild> : BaseAsyncRewardHelper where TParent : OwnerQuantityObjectList<TChild> where TChild : OwnerQuantityChild, IId, new()
    {      
        public override async Task GiveRewardsAsync(WebContext context, long entityId, long quantity, object extraData, RewardParams rp)
        {
            await _serverRewardService.AddQuantity<TChild>(context, entityId, quantity, rp);
        }
    }
}
