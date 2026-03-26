using Genrpg.DataUtils.Entities.Core;
using Genrpg.DataUtils.Importers.Core;
using Genrpg.Shared.Attributes.Settings;

namespace Genrpg.DataUtils.Importers.Gameplay
{
    public class GameplayStatImporter : ParentChildImporter<GameplayStatSettings, GameplayStat>
    {
        protected override void ImportChildSubObject(EditorGameState gs, GameplayStat current, int row, string firstColumn, string[] headers, string[] rowWords)
        {
        }
    }
}
