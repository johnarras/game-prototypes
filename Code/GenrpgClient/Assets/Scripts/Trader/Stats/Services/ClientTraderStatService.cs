

using Assets.Scripts.Trader.ClientEvents;
using Genrpg.Shared.Attributes.Services;
using Genrpg.Shared.DataStores.Categories.PlayerData.Units;
using System.Threading.Tasks;

namespace Assets.Scripts.Trader.Stats.Services
{
    public class ClientAttributeService : AttributeService
    {
        private IDispatcher _dispatcher = null;

        public override async Task UpdateBuffsAndDebuffs(IUnitDataLookup lookup)
        {
            await base.UpdateBuffsAndDebuffs(lookup);
            _dispatcher.Dispatch(new UpdateTraderHUD());
        }

        public override async Task AddDebuffDaysPlayed(IUnitDataLookup lookup, long daysAdded)
        {
            await base.AddDebuffDaysPlayed(lookup, daysAdded);
            _dispatcher.Dispatch(new UpdateTraderHUD());
        }
    }
}
