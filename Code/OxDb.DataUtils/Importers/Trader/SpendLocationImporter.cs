using OxDb.DataUtils.Entities.Core;
using OxDb.DataUtils.Importers.Core;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Trader.CurrencySpend.Settings;

namespace OxDb.DataUtils.Importers.Trader
{
    public class SpendLocationImporter : ParentChildImporter<SpendLocationSettings, SpendLocation>
    {
        protected override void ImportChildSubObject(EditorGameState gs, SpendLocation current, int row, string firstColumn, string[] headers, string[] rowWords)
        {
            if (current == null)
            {
                return;
            }

            if (firstColumn == StrUtils.NormalizeTypeName<SpendType>())
            {
                current.SpendTypes.Add(_importService.ImportLine<SpendType>(gs, row, headers, rowWords));
            }
            else if (firstColumn == StrUtils.NormalizeTypeName<SpendReward>())
            {
                current.SpendTypes.Last().Rewards.Add(_importService.ImportLine<SpendReward>(gs, row, headers, rowWords));
            }

        }
    }
}
