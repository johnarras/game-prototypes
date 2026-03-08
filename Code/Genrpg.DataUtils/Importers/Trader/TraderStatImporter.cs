using Genrpg.DataUtils.Entities.Core;
using Genrpg.DataUtils.Importers.Core;
using Genrpg.Shared.Trader.Stats.Settings;

namespace Genrpg.DataUtils.Importers.Trader
{
    public class TraderStatImporter : ParentChildImporter<TraderStatSettings, TraderStat>
    {
        protected override void ImportChildSubObject(EditorGameState gs, TraderStat current, int row, string firstColumn, string[] headers, string[] rowWords)
        {
        }
    }
}
