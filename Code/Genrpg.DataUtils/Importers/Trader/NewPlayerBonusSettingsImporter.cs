using Genrpg.DataUtils.Entities.Core;
using Genrpg.DataUtils.Importers.Core;
using Genrpg.Shared.NewPlayers.Settings;

namespace Genrpg.DataUtils.Importers.Trader
{
    public class NewPlayerBonusSettingsImporter : ParentChildImporter<NewPlayerBonusSettings, NewPlayerBonus>
    {
        protected override void ImportChildSubObject(EditorGameState gs, NewPlayerBonus current, int row, string firstColumn, string[] headers, string[] rowWords)
        {
        }
    }
}


