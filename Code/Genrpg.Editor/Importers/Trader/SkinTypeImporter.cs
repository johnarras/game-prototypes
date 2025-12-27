using Genrpg.Editor.Constants;
using Genrpg.Editor.Entities.Core;
using Genrpg.Editor.Importers.Core;
using Genrpg.Shared.Trader.Animals.Settings;

namespace Genrpg.Editor.Importers.Trader
{
    public class SkinTypeImporter : ParentChildImporter<SkinTypeSettings, SkinType>
    {
        public override string ImportDataFilename => "SkinTypeImport.csv";

        public override EImportTypes HelperKey => EImportTypes.SkinTypes;

        protected override void ImportChildSubObject(EditorGameState gs, SkinType current, int row, string firstColumn, string[] headers, string[] rowWords)
        {
        }
    }
}
