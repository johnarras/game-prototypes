using OxDb.DataUtils.Entities.Core;
using OxDb.DataUtils.Importers.Core;
using OxDb.SharedGame.Minigames.Games.Settings;

namespace OxDb.DataUtils.Importers.Minigames
{
    public class MinigameTypeImporter : ParentChildImporter<MinigameTypeSettings, MinigameType>
    {
        protected override void ImportChildSubObject(EditorGameState gs, MinigameType current, int row, string firstColumn, string[] headers, string[] rowWords)
        {
        }
    }
}
