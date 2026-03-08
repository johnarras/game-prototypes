using Genrpg.DataUtils.Entities.Core;
using Genrpg.DataUtils.Importers.Core;
using Genrpg.Shared.Spawns.Settings;
using Genrpg.Shared.Trader.Cultures.Settings;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.DataUtils.Importers.Trader
{
    public class TravelEncounterSettingsImporter : ParentChildImporter<TravelEncounterSettings, TravelEncounter>
    {
        protected override void ImportChildSubObject(EditorGameState gs, TravelEncounter current, int row, string firstColumn, string[] headers, string[] rowWords)
        {
            if (string.IsNullOrEmpty(firstColumn))
            {
                return;
            }
            firstColumn = firstColumn.ToLower();

            if (firstColumn == "goodeffect")
            {
                current.GoodEffects.Add(_importService.ImportLine<SpawnItem>(gs, row, headers, rowWords));
            }
            else if (firstColumn == "badeffect")
            {
                current.BadEffects.Add(_importService.ImportLine<SpawnItem>(gs, row, headers, rowWords));
            }
            else if (firstColumn == "failureeffect")
            {
                current.FailureEffects.Add(_importService.ImportLine<SpawnItem>(gs, row, headers, rowWords));
            }
        }
    }
}
