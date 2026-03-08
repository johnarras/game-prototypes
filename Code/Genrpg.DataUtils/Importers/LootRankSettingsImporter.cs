using Genrpg.DataUtils.Entities.Core;
using Genrpg.DataUtils.Importers.Core;
using Genrpg.Shared.Inventory.Settings.Ranks;

namespace Genrpg.DataUtils.Importers
{
    public class LootRankSettingsImporter : ParentChildImporter<LootRankSettings, LootRank>
    {
        protected override void ImportChildSubObject(EditorGameState gs, LootRank current, int row, string firstColumn, string[] headers, string[] rowWords)
        {
        }
    }
}


