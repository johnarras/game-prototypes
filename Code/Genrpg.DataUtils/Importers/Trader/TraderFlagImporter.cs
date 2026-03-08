using Genrpg.DataUtils.Entities.Core;
using Genrpg.DataUtils.Importers.Core;
using Genrpg.Shared.Trader.Flags.Settings;

namespace Genrpg.DataUtils.Importers.Trader
{
    public class TraderFlagImporter : ParentChildImporter<TraderFlagSettings, TraderFlag>
    {
        protected override void ImportChildSubObject(EditorGameState gs, TraderFlag current, int row, string firstColumn, string[] headers, string[] rowWords)
        {
        }
    }
}
