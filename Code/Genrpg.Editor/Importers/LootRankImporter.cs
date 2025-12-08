using Genrpg.Editor.Constants;
using Genrpg.Editor.Entities.Core;
using Genrpg.Editor.Importers.Core;
using Genrpg.Shared.Inventory.Settings.Ranks;

namespace Genrpg.Editor.Importers
{
    public class LootRankImporter : ParentChildImporter<LootRankSettings, LootRank>
    {
        public override string ImportDataFilename => "LootRankImport.csv";

        public override EImportTypes HelperKey => EImportTypes.LootRanks;

        protected override void ImportChildSubObject(EditorGameState gs, LootRank current, int row, string firstColumn, string[] headers, string[] rowWords)
        {
        }
    }
}
