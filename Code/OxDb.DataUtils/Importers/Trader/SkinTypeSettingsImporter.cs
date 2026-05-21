using OxDb.DataUtils.Entities.Core;
using OxDb.DataUtils.Importers.Core;
using OxDb.SharedGame.Trader.CaravanMembers.Settings;

namespace OxDb.DataUtils.Importers.Trader
{
    public class SkinTypeSettingsImporter : ParentChildImporter<SkinTypeSettings, SkinType>
    {
        protected override void ImportChildSubObject(EditorGameState gs, SkinType current, int row, string firstColumn, string[] headers, string[] rowWords)
        {
        }
    }
}
