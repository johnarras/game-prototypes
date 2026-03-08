using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Units.Settings;

namespace Genrpg.DataUtils.Importers.Crawler
{
    public class UnitKeywordSettingsImporter : BaseUnitDataImporter<UnitKeywordSettings, UnitKeyword>
    {
        public override long GetEntityTypeId() { return EntityTypes.UnitKeyword; }
    }
}


