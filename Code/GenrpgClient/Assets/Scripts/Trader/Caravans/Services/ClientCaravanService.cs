using Assets.Scripts.Trader.ClientEvents;
using Assets.Scripts.Trader.Travel.ClientEvents;
using Assets.Scripts.Core;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.Trader.Caravans.Services;
using System.Threading.Tasks;
using Genrpg.Shared.DataStores.Categories.PlayerData.Units;

namespace Assets.Scripts.Trader.Caravans.Services
{
    public class ClientCaravanService : CaravanService
    {
        private IDispatcher _dispatcher = null;
        public override async Task CalcCoreTravelStats(IUnitDataLookup lookup)
        {
            await base.CalcCoreTravelStats(lookup); 

            _dispatcher.Dispatch(new UpdateTraderHUD());

        }
    }
}
