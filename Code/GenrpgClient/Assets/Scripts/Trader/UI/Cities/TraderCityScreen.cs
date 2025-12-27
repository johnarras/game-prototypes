using Assets.Scripts.UI.ScreenSystem;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.Trader.Caravans.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.Trader.UI.Cities
{
    public class TraderCityScreen : TypedArgScreen<TraderCityScreenArgs>
    {

        protected override async Task OnStartOpen(TraderCityScreenArgs args, CancellationToken token)
        {

            CoreUserData userData = _gs.ch.Get<CoreUserData>();

            CaravanPosition pos = userData.GetPosition();


            await Task.CompletedTask;
        }
    }
}


