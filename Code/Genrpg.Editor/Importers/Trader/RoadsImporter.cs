using Genrpg.Editor.Constants;
using Genrpg.Editor.Entities.Core;
using Genrpg.Shared.Trader.Roads.Settings;

namespace Genrpg.Editor.Importers.Trader
{
    public class RoadsImporter : BaseTraderDataImporter<RoadSettings, Road>
    {
        public override string ImportDataFilename => "RoadsImport.csv";

        public override EImportTypes Key => EImportTypes.Roads;

        protected override void ImportChildSubObject(EditorGameState gs, Road current, int row, string firstColumn, string[] headers, string[] rowWords)
        {

        }
    }
}
