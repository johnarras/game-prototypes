using Genrpg.Editor.Entities.Core;
using Genrpg.Editor.Importers.Core;
using Genrpg.Shared.NewPlayers.Settings;

namespace Genrpg.Editor.Importers.Trader
{
    public class NewPlayerBonusSettingsImporter : ParentChildImporter<NewPlayerBonusSettings, NewPlayerBonus>
    {
        protected override void ImportChildSubObject(EditorGameState gs, NewPlayerBonus current, int row, string firstColumn, string[] headers, string[] rowWords)
        {
        }
    }
}


