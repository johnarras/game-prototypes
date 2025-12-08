using Genrpg.Editor.Constants;
using Genrpg.Editor.Entities.Core;
using Genrpg.Editor.Importers.Core;
using Genrpg.Shared.NewPlayers.Settings;

namespace Genrpg.Editor.Importers.Trader
{
    public class NewPlayerBonusesImporter : ParentChildImporter<NewPlayerBonusSettings, NewPlayerBonus>
    {
        public override string ImportDataFilename => "NewPlayerBonusesImport.csv";

        public override EImportTypes HelperKey => EImportTypes.NewPlayerBonuses;

        protected override void ImportChildSubObject(EditorGameState gs, NewPlayerBonus current, int row, string firstColumn, string[] headers, string[] rowWords)
        {
        }
    }
}
