using Genrpg.Editor.Constants;
using Genrpg.Editor.Entities.Core;
using Genrpg.Shared.Trader.Animals.Settings;

namespace Genrpg.Editor.Importers.Trader
{
    public class AnimalsImporter : BaseTraderDataImporter<AnimalSettings, Animal>
    {
        public override string ImportDataFilename => "AnimalsImport.csv";

        public override EImportTypes Key => EImportTypes.Animals;

        protected override void ImportChildSubObject(EditorGameState gs, Animal current, int row, string firstColumn, string[] headers, string[] rowWords)
        {

        }
    }
}
