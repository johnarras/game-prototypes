using OxDb.DataUtils.Entities.Core;
using OxDb.DataUtils.Importers.Core;
using OxDb.SharedGame.Trader.Flags.Settings;

namespace OxDb.DataUtils.Importers.Trader
{
    public class TraderFlagImporter : ParentChildImporter<TraderFlagSettings, TraderFlag>
    {
        protected override void ImportSubobject(EditorGameState gs, TraderFlagSettings settings, TraderFlag current, int row, string firstColumn, string[] headers, string[] rowWords)
        {
        }
    }
}
