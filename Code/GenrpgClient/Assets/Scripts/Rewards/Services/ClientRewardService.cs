using OxDb.Client.DynamicUI.Services;
using OxDb.Client.Trader.Travel.UI;
using OxDb.SharedCore.Logalytics.Constants;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedCore.Rewards.Entities;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Units;
using OxDb.SharedGame.Inventory.PlayerData;
using OxDb.SharedGame.Rewards.Services;
using System.Threading.Tasks;

namespace OxDb.Client.Rewards.Services
{
    public class ClientRewardService : RewardService
    {
        private IDispatcher _dispatcher;
        private IDynamicUIService _dynamicUIService = null;
        private IAnalyticsService _analyticsService = null;

        public override async ValueTask<bool> GiveReward(IUnitDataLookup obj, long entityTypeId, long entityId, long quantity, long rewardSourceId, Item extraData, long uniqueId, RewardParams rp)
        {
            if (await base.GiveReward(obj, entityTypeId, entityId, quantity, rewardSourceId, extraData, uniqueId, rp))
            {
                ClientRewardParams crp = rp as ClientRewardParams;
                bool showDoober = crp?.ShowDoobers ?? true;

                bool showVisualUpdate = crp?.ShowVisualUpdate ?? true;

                bool instant = crp?.InstantShow ?? false;

                if (quantity != 0 && (crp == null || !crp.SuppressAnalytics))
                {
                    _analyticsService.TrackEconomyEvent(quantity > 0 ? AnalyticsEventNames.RewardInflow : AnalyticsEventNames.RewardOutflow,
                        entityTypeId, entityId, quantity, rewardSourceId);
                }

                _dispatcher.Dispatch(new UpdateMaxPlayMult());

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
        public bool SuppressAnalytics { get; set; } = false;

        public bool InstantShow { get; set; }
        public ClientRewardParams(bool showDoobers, bool showVisualUpdate, bool suppressAnalytics)
        {
            ShowDoobers = showDoobers;
            ShowVisualUpdate = showVisualUpdate;
            SuppressAnalytics = suppressAnalytics;
        }
    }
}


