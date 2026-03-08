using Genrpg.DataUtils.Entities.Core;
using Genrpg.DataUtils.Importers.Core;
using Genrpg.Shared.Spawns.Settings;
using Genrpg.Shared.Tiles.Settings;

namespace Genrpg.DataUtils.Importers.BoardGame
{
    public class TileTypeSettingsImporter : ParentChildImporter<TileTypeSettings, TileType>
    {
        class TileReagentRow
        {
            public long Stone { get; set; }
            public long Iron { get; set; }
            public long Wood { get; set; }
            public long Food { get; set; }
            public long Herbs { get; set; }
            public long Leather { get; set; }
            public long Sum { get; set; }
        }

        protected override void ImportChildSubObject(EditorGameState gs, TileType current, int line, string firstColumn, string[] headers, string[] rowWords)
        {
            if (firstColumn == "reward")
            {
                current.Rewards.Add(_importService.ImportLine<SpawnItem>(gs, line, headers, rowWords));
            }
            else if (firstColumn == "effect")
            {
                current.Effects.Add(_importService.ImportLine<TileEffect>(gs, line, headers, rowWords));
            }
            else if (firstColumn == "reagents")
            {
                TileReagentRow reagents = _importService.ImportLine<TileReagentRow>(gs, line, headers, rowWords);

                if (reagents != null)
                {
                    current.UpgradeReagents.Clear();
                }
            }
        }
    }
}


