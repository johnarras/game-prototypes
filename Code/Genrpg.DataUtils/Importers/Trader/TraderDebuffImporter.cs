using Genrpg.DataUtils.Entities.Core;
using Genrpg.DataUtils.Importers.Core;
using Genrpg.Shared.Trader.Stats.Settings;

namespace Genrpg.DataUtils.Importers.Trader
{
    public class TraderDebuffImporter : ParentChildImporter<TraderDebuffSettings, TraderDebuff>
    {
        protected override void ImportChildSubObject(EditorGameState gs, TraderDebuff current, int row, string firstColumn, string[] headers, string[] rowWords)
        {
            if (firstColumn == "debuffeffect")
            {
                current.Effects.Add(_importService.ImportLine<DebuffEffect>(gs, row, headers, rowWords));
            }
        }
    }
}
