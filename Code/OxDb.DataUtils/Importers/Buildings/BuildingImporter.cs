using OxDb.DataUtils.Entities.Core;
using OxDb.DataUtils.Importers.Core;
using OxDb.SharedGame.Buildings.Settings;

namespace OxDb.DataUtils.Importers.Buildings
{
    public class BuildingImporter : ParentChildImporter<BuildingSettings, BuildingType>
    {
        protected override void ImportSubobject(EditorGameState gs, BuildingSettings settings, BuildingType current, int row, string firstColumn, string[] headers, string[] rowWords)
        {
        }
    }
}
