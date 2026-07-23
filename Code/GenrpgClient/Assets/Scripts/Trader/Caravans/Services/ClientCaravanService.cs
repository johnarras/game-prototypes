using OxDb.Client.Trader.ClientEvents;
using OxDb.SharedGame.Attributes.Services;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Units;
using System.Threading.Tasks;

namespace OxDb.Client.Trader.Caravans.Services
{
    public class ClientCalcAttributeService : CalcAttributeService
    {
        private IDispatcher _dispatcher = null;
        public override async ValueTask CalcBuffs(IUnitDataLookup lookup)
        {
            await base.CalcBuffs(lookup);

            _dispatcher.Dispatch(new UpdateTraderHUD());

        }
    }
}
