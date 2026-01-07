using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Units.Settings;

namespace Genrpg.Editor.Importers.Crawler
{
    public class UnitTypeSettingsImporter : BaseUnitDataImporter<UnitTypeSettings, UnitType>
    {
        public override long GetEntityTypeId() { return EntityTypes.Unit; }
    }
}


