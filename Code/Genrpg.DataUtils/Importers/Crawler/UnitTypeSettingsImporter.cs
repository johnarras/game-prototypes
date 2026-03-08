using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Units.Settings;

namespace Genrpg.DataUtils.Importers.Crawler
{
    public class UnitTypeSettingsImporter : BaseUnitDataImporter<UnitTypeSettings, UnitType>
    {
        public override long GetEntityTypeId() { return EntityTypes.Unit; }
    }
}


