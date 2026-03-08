using Assets.Scripts.DynamicUI.Services;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Rewards.Services;
using Genrpg.Shared.Utils;

namespace Assets.Scripts.Rewards.Services
{
    public class ClientRewardService : RewardService
    {
        private IDispatcher _dispatcher;
        private IDynamicUIService _dynamicUIService = null;

        public override bool GiveReward(IRandom rand, MapObject obj, long entityTypeId, long entityId, long quantity, object extraData, RewardParams rp)
        {
            if (base.GiveReward(rand, obj, entityTypeId, entityId, quantity, extraData, rp))
            {

                ClientRewardParams crp = rp as ClientRewardParams;
                bool showDoober = crp?.ShowDoobers ?? true;

                bool showVisualUpdate = crp?.ShowVisualUpdate ?? true;

                bool instant = crp?.InstantShow ?? false;

                if (showDoober && quantity > 0 && _dynamicUIService.ShowDefaultEntityDoober(entityTypeId, entityId, quantity))
                {
                    return true;
                }
                else if (showVisualUpdate)
                {
                    _dynamicUIService.AddEntityQuantityVisual(entityTypeId, entityId, quantity, instant);
                } 
            }

            return false;
        }
    }

    public class ClientRewardParams : RewardParams
    {
        public bool ShowDoobers { get; set; } = true;
        public bool ShowVisualUpdate { get; set; } = true;

        public bool InstantShow { get; set; }
        public ClientRewardParams(bool showDoobers, bool showVisualUpdate)  
        {
            ShowDoobers = showDoobers;
            ShowVisualUpdate = showVisualUpdate;
        }
    }
}


