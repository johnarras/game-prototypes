using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedGame.Units.Settings;

namespace OxDb.DataUtils.Importers.Crawler
{
    public class UnitTypeSettingsImporter : BaseUnitDataImporter<UnitTypeSettings, UnitType>
    {
        public override long GetEntityTypeId() { return EntityTypes.Unit; }
    }
}


