using Genrpg.Editor.Entities.Core;
using Genrpg.Shared.Trader.Roads.Settings;

namespace Genrpg.Editor.Importers.Trader
{
    public class RoadSettingsImporter : BaseTraderDataImporter<RoadSettings, Road>
    {
        protected override void ImportChildSubObject(EditorGameState gs, Road current, int row, string firstColumn, string[] headers, string[] rowWords)
        {

        }
    }
}


