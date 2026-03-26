using Assets.Scripts.UI.ScreenSystem;
using Genrpg.Shared.Attributes.PlayerData;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.Trader.Caravans.PlayerData;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.Trader.UI.TradeGoods
{
    public class TradeGoodsScreen : BaseScreen
    {
        protected override async Task OnStartOpen(object data, CancellationToken token)
        {
            CaravanData cdata = _gs.ch.Get<CaravanData>();
            CoreData coreData = _gs.ch.Get<CoreData>();

            AttributeData attributeData = _gs.ch.Get<AttributeData>();

            await Task.CompletedTask;
        }
    }
}


