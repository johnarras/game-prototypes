using Assets.Scripts.ClientEvents.Entities;
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
            if (entityTypeId == EntityTypes.CoreCurrency && (rp == null || !rp.SkipVisualUpdate))
            {
                // Use doobers instead?
                _dispatcher.Dispatch(new AddEntityQuantityVisual() { EntityTypeId = entityTypeId, InstantUpdate = false, QuantityAdded = diff, EntityId = entityId });
            }
        }
    }
}
