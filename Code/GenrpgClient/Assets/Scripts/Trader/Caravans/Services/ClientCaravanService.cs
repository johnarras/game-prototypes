using Assets.Scripts.Trader.Travel.ClientEvents;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.Trader.Caravans.Services;

namespace Assets.Scripts.Trader.Caravans.Services
{
    public class ClientCaravanService : CaravanService
    {
        private IDispatcher _dispatcher = null;
        public override void UpdateTravelStats(CoreData core)
        {
            base.UpdateTravelStats(core);

            _dispatcher.Dispatch(new VisualUpdateTravelStats());

        }
    }
}
