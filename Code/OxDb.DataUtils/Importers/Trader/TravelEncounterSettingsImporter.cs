using OxDb.DataUtils.Entities.Core;
using OxDb.DataUtils.Importers.Core;
using OxDb.SharedGame.Spawns.Settings;
using OxDb.SharedGame.Trader.Encounters.Settings;

namespace OxDb.DataUtils.Importers.Trader
{
    public class TravelEncounterSettingsImporter : ParentChildImporter<TravelEncounterSettings, TravelEncounter>
    {
        protected override void ImportSubobject(EditorGameState gs, TravelEncounterSettings settings, TravelEncounter current, int row, string firstColumn, string[] headers, string[] rowWords)
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
