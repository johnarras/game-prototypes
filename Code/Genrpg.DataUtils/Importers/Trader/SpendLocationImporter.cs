using Genrpg.DataUtils.Entities.Core;
using Genrpg.DataUtils.Importers.Core;
using Genrpg.Shared.Trader.CurrencySpend.Settings;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.DataUtils.Importers.Trader
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
