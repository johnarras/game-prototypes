using Assets.Scripts.Trader.ClientEvents;
using Genrpg.Shared.Attributes.Services;
using Genrpg.Shared.DataStores.Categories.PlayerData.Units;
using System.Threading.Tasks;

namespace Assets.Scripts.Trader.Caravans.Services
{
    public class ClientCalcAttributeService : CalcAttributeService
    {
        private IDispatcher _dispatcher = null;
        public override async Task CalcBuffs(IUnitDataLookup lookup)
        {
            await base.CalcBuffs(lookup);

            _dispatcher.Dispatch(new UpdateTraderHUD());

        }
    }
}
