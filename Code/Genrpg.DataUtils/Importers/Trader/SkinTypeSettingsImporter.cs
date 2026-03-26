using Genrpg.DataUtils.Entities.Core;
using Genrpg.DataUtils.Importers.Core;
using Genrpg.Shared.Trader.CaravanMembers.Settings;
using Genrpg.Shared.Utils;

namespace Genrpg.DataUtils.Importers.Trader
{
    public class SkinTypeSettingsImporter : ParentChildImporter<SkinTypeSettings, SkinType>
    {
        protected override void ImportChildSubObject(EditorGameState gs, SkinType current, int row, string firstColumn, string[] headers, string[] rowWords)
        {
        }
    }
}
