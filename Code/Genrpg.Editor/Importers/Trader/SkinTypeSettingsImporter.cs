using Genrpg.Editor.Entities.Core;
using Genrpg.Editor.Importers.Core;
using Genrpg.Shared.Trader.Animals.Settings;

namespace Genrpg.Editor.Importers.Trader
{
    public class SkinTypeSettingsImporter : ParentChildImporter<SkinTypeSettings, SkinType>
    {
        protected override void ImportChildSubObject(EditorGameState gs, SkinType current, int row, string firstColumn, string[] headers, string[] rowWords)
        {
        }
    }
}
