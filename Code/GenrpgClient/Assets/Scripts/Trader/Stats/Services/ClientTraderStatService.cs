using Assets.Scripts.Trader.ClientEvents;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.Trader.Caravans.PlayerData;
using Genrpg.Shared.Trader.Stats.PlayerData;
using Genrpg.Shared.Trader.Stats.Services;

namespace Assets.Scripts.Trader.Stats.Services
{
   public class ClientTraderStatService : TraderStatService
    {
        private IDispatcher _dispatcher = null;

        public override void UpdateStats(CoreData coreData, CaravanData caravanData, TraderStatData statData)
        {
           base.UpdateStats(coreData, caravanData, statData);
            _dispatcher.Dispatch(new UpdateTraderHUD());
        }

        public override void AddDebuffDaysPlayed(CoreData coreData, CaravanData caravanData, TraderStatData statData, long daysAdded)
        {
            base.AddDebuffDaysPlayed(coreData, caravanData, statData, daysAdded);
            _dispatcher.Dispatch(new UpdateTraderHUD());
        }
    }
}
