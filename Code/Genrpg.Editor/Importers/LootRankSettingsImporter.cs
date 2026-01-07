using Genrpg.Editor.Entities.Core;
using Genrpg.Editor.Importers.Core;
using Genrpg.Shared.Inventory.Settings.Ranks;

namespace Genrpg.Editor.Importers
{
    public class LootRankSettingsImporter : ParentChildImporter<LootRankSettings, LootRank>
    {
        protected override void ImportChildSubObject(EditorGameState gs, LootRank current, int row, string firstColumn, string[] headers, string[] rowWords)
        {
        }
    }
}


