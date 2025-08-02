using Genrpg.Editor.Constants;
using Genrpg.Editor.Entities.Core;
using Genrpg.Editor.Importers.Core;
using Genrpg.Shared.Entities.Utils;
using Genrpg.Shared.Spawns.Settings;
using Genrpg.Shared.Tiles.Settings;
using Genrpg.Shared.UserCoins.Settings;

namespace Genrpg.Editor.Importers.BoardGame
{
    public class TileTypeImporter : ParentChildImporter<TileTypeSettings, TileType>
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


        public override string ImportDataFilename => "TileTypeImport.csv";

        public override EImportTypes Key => EImportTypes.TileTypes;

        protected override void ImportChildSubObject(EditorGameState gs, TileType current, int line, string firstColumn, string[] headers, string[] rowWords)
        {
            if (firstColumn == "reward")
            {
                current.Rewards.Add(_importService.ImportLine<SpawnItem>(gs, line, rowWords, headers));
            }
            else if (firstColumn == "effect")
            {
                current.Effects.Add(_importService.ImportLine<TileEffect>(gs, line, rowWords, headers));
            }
            else if (firstColumn == "reagents")
            {
                TileReagentRow reagents = _importService.ImportLine<TileReagentRow>(gs, line, rowWords, headers);

                if (reagents != null)
                {
                    current.UpgradeReagents.Clear();

                    foreach (UserCoinType coinType in gs.data.Get<UserCoinSettings>(null).GetData())
                    {
                        int quantity = EntityUtils.GetObjectInt(reagents, coinType.Name);

                        if (quantity > 0)
                        {
                            current.UpgradeReagents.Add(new TileUpgradeReagent() { Quantity = quantity, UserCoinTypeId = coinType.IdKey });
                        }
                    }
                }
            }
        }
    }
}
