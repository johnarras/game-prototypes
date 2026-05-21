using OxDb.DataUtils.Entities.Core;
using OxDb.DataUtils.Importers.Core;
using OxDb.SharedGame.Trader.Flags.Settings;

namespace OxDb.DataUtils.Importers.Trader
{
    public class TraderFlagImporter : ParentChildImporter<TraderFlagSettings, TraderFlag>
    {
        protected override void ImportChildSubObject(EditorGameState gs, TraderFlag current, int row, string firstColumn, string[] headers, string[] rowWords)
        {
        }
    }
}
