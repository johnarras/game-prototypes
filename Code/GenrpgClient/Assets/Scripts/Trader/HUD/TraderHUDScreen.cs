using OxDb.Client.ClientEvents.UI;
using OxDb.Client.Trader.WorldMap.Services;
using OxDb.SharedGame.Trader.Caravans.Entities;
using OxDb.SharedGame.Trader.Caravans.Services;
using OxDb.SharedGame.UI.Constants;
using System.Threading;
using System.Threading.Tasks;

namespace OxDb.Client.Trader.UI.TraderHUD
{
    public class TraderHUDScreen : BaseScreen
    {
        private ICaravanService _caravanService = null;
        private ITraderTerrainService _terrainService = null;
        protected override async Task OnStartOpen(object data, CancellationToken token)
        {

            _terrainService.ShowTerrain();

            CaravanPosition pos = await _caravanService.GetPosition(_gs.ch);

            if (pos.GetCurrentCity() != null)
            {
                _dispatcher.Dispatch(new OpenScreen(ScreenNames.TraderCity));
            }

            await Task.CompletedTask;
        }

        protected override void OnStartClose()
        {
            base.OnStartClose();

            _terrainService.HideTerrain();

        }
    }
}


