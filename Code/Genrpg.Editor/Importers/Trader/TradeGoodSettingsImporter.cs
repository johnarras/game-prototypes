using Genrpg.Editor.Entities.Core;
using Genrpg.Shared.Trader.TradeGoods.Settings;

namespace Genrpg.Editor.Importers.Trader
{
    public class TradeGoodSettingsImporter : BaseTraderDataImporter<TradeGoodSettings, TradeGood>
    {
        protected override void ImportChildSubObject(EditorGameState gs, TradeGood current, int row, string firstColumn, string[] headers, string[] rowWords)
        {

        }
    }
}


