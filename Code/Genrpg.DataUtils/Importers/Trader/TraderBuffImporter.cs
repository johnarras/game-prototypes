using Genrpg.DataUtils.Entities.Core;
using Genrpg.DataUtils.Importers.Core;
using Genrpg.Shared.Trader.Stats.Settings;

namespace Genrpg.DataUtils.Importers.Trader
{
    public class TraderBuffImporter : ParentChildImporter<TraderBuffSettings, TraderBuff>
    {
        protected override void ImportChildSubObject(EditorGameState gs, TraderBuff current, int row, string firstColumn, string[] headers, string[] rowWords)
        {
            if (firstColumn == "buffeffect")
            {
                current.Effects.Add(_importService.ImportLine<BuffEffect>(gs, row, headers, rowWords));
            }
        }
    }
}
