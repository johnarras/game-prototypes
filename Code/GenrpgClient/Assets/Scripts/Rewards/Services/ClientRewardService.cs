using Assets.Scripts.ClientEvents.UserCoins;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Rewards.Services;

namespace Assets.Scripts.Rewards.Services
{
    public class ClientRewardService : RewardService
    {
        private IDispatcher _dispatcher;
        public override void OnAddQuantity<TUpd>(MapObject obj, TUpd upd, long entityTypeId, long entityId, long diff, RewardParams rp)
        {
            if (entityTypeId == EntityTypes.UserCoin && (rp == null || !rp.SkipVisualUpdate))
            {
                // Use doobers instead?
                _dispatcher.Dispatch(new AddUserCoinVisual() { InstantUpdate = false, QuantityAdded = diff, UserCoinTypeId = entityId });
            }
        }
    }
}
