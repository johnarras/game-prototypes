using Assets.Scripts.UI.ScreenSystem;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.Trader.Caravans.PlayerData;
using Genrpg.Shared.Trader.Stats.PlayerData;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.Trader.UI.TradeGoods
{
    public class TradeGoodsScreen : BaseScreen
    {
        protected override async Task OnStartOpen(object data, CancellationToken token)
        {
            CaravanData cdata = _gs.ch.Get<CaravanData>();
            CoreUserData coreData = _gs.ch.Get<CoreUserData>();

            TraderStatData statData = _gs.ch.Get<TraderStatData>();

            await Task.CompletedTask;
        }
    }
}


