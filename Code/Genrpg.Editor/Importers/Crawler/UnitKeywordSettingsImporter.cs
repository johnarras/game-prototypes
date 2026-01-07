using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Units.Settings;

namespace Genrpg.Editor.Importers.Crawler
{
    public class UnitKeywordSettingsImporter : BaseUnitDataImporter<UnitKeywordSettings, UnitKeyword>
    {
        public override long GetEntityTypeId() { return EntityTypes.UnitKeyword; }
    }
}


