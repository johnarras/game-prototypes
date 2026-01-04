using Assets.Scripts.UI.ScreenSystem;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.Trader.Caravans.Entities;
using Genrpg.Shared.Trader.Caravans.Services;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.Trader.UI.Cities
{
    public class TraderCityScreen : TypedArgScreen<TraderCityScreenArgs>
    {

        private ICaravanService _caravanService = null;

        protected override async Task OnStartOpen(TraderCityScreenArgs args, CancellationToken token)
        {

            CoreData coreData = _gs.ch.Get<CoreData>();

            CaravanPosition pos = _caravanService.GetPosition(coreData);


            await Task.CompletedTask;
        }
    }
}


