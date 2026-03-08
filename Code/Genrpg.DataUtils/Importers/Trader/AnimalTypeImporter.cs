using Genrpg.DataUtils.Entities.Core;
using Genrpg.Shared.Trader.Animals.Settings;

namespace Genrpg.DataUtils.Importers.Trader
{
    public class AnimalTypeSettingsImporter : BaseTraderDataImporter<AnimalTypeSettings, AnimalType>
    {
        protected override void ImportChildSubObject(EditorGameState gs, AnimalType current, int row, string firstColumn, string[] headers, string[] rowWords)
        {

        }
    }
}


