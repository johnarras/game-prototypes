using OxDb.DataUtils.Entities.Core;
using OxDb.DataUtils.Importers.Core;
using OxDb.SharedGame.Attributes.Settings;

namespace OxDb.DataUtils.Importers.Gameplay
{
    public class GameplayStatImporter : ParentChildImporter<GameplayStatSettings, GameplayStat>
    {
        protected override void ImportSubobject(EditorGameState gs, GameplayStatSettings settings, GameplayStat current, int row, string firstColumn, string[] headers, string[] rowWords)
        {
        }
    }
}
