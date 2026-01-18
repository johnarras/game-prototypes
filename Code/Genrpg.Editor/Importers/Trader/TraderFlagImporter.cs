using Genrpg.Editor.Entities.Core;
using Genrpg.Editor.Importers.Core;
using Genrpg.Shared.Trader.Flags.Settings;

namespace Genrpg.Editor.Importers.Trader
{
    public class TraderFlagImporter : ParentChildImporter<TraderFlagSettings, TraderFlag>
    {
        protected override void ImportChildSubObject(EditorGameState gs, TraderFlag current, int row, string firstColumn, string[] headers, string[] rowWords)
        {
        }
    }
}
