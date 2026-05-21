using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedGame.Units.Settings;

namespace OxDb.DataUtils.Importers.Crawler
{
    public class UnitKeywordSettingsImporter : BaseUnitDataImporter<UnitKeywordSettings, UnitKeyword>
    {
        public override long GetEntityTypeId() { return EntityTypes.UnitKeyword; }
    }
}


