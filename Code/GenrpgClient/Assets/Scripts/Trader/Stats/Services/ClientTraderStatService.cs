

using Assets.Scripts.Trader.ClientEvents;
using OxDb.SharedGame.Attributes.Services;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Units;
using System.Threading.Tasks;

namespace Assets.Scripts.Trader.Stats.Services
{
    public class ClientAttributeService : AttributeService
    {
        private IDispatcher _dispatcher = null;

        public override async ValueTask UpdateBuffsAndDebuffs(IUnitDataLookup lookup)
        {
            await base.UpdateBuffsAndDebuffs(lookup);
            _dispatcher.Dispatch(new UpdateTraderHUD());
        }

        public override async ValueTask AddDebuffDaysPlayed(IUnitDataLookup lookup, long daysAdded)
        {
            await base.AddDebuffDaysPlayed(lookup, daysAdded);
            _dispatcher.Dispatch(new UpdateTraderHUD());
        }
    }
}
