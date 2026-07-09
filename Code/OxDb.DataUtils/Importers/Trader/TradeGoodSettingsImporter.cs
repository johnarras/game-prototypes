using OxDb.DataUtils.Entities.Core;
using OxDb.SharedGame.Trader.TradeGoods.Settings;

namespace OxDb.DataUtils.Importers.Trader
{
    public class TradeGoodSettingsImporter : BaseTraderDataImporter<TradeGoodSettings, TradeGood>
    {
        protected override void ImportSubobject(EditorGameState gs, TradeGoodSettings settings, TradeGood current, int row, string firstColumn, string[] headers, string[] rowWords)
        {

        }
    }
}


