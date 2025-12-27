using Genrpg.Editor.Constants;
using Genrpg.Editor.Entities.Core;
using Genrpg.Shared.Trader.Animals.Settings;

namespace Genrpg.Editor.Importers.Trader
{
    public class AnimalsImporter : BaseTraderDataImporter<AnimalTypeSettings, AnimalType>
    {
        public override string ImportDataFilename => "AnimalTypeImport.csv";

        public override EImportTypes HelperKey => EImportTypes.Animals;

        protected override void ImportChildSubObject(EditorGameState gs, AnimalType current, int row, string firstColumn, string[] headers, string[] rowWords)
        {

        }
    }
}


