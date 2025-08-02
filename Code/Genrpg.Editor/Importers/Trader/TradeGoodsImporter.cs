using Genrpg.Editor.Constants;
using Genrpg.Editor.Entities.Core;
using Genrpg.Shared.Trader.TradeGoods.Settings;

namespace Genrpg.Editor.Importers.Trader
{
    public class TradeGoodsImporter : BaseTraderDataImporter<TradeGoodSettings, TradeGood>
    {
        public override string ImportDataFilename => "TradeGoodsImport.csv";

        public override EImportTypes Key => EImportTypes.TradeGoods;

        protected override void ImportChildSubObject(EditorGameState gs, TradeGood current, int row, string firstColumn, string[] headers, string[] rowWords)
        {

        }
    }
}
