using OxDb.DataUtils.Entities.Core;
using OxDb.DataUtils.Importers.Core;
using OxDb.SharedGame.Inventory.Settings.Ranks;

namespace OxDb.DataUtils.Importers.Items
{
    public class LootRankSettingsImporter : ParentChildImporter<LootRankSettings, LootRank>
    {
        protected override void ImportSubobject(EditorGameState gs, LootRankSettings settings, LootRank current, int row, string firstColumn, string[] headers, string[] rowWords)
        {
        }
    }
}


