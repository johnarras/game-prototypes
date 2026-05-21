using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;
using OxDb.SharedGame.Zones.Constants;
using System.Collections.Generic;

namespace OxDb.SharedGame.Zones.Settings
{
    public class ZoneTypeSettings : ParentConstantListSettings<ZoneType, ZoneTypes>
    {
        public override string Id { get; set; }
    }

    public class ZoneTypeSettingsDto : ParentSettingsDto<ZoneTypeSettings, ZoneType>
    {
        public override string Id { get; set; }
        public override List<ZoneType> Children { get; set; } = new List<ZoneType>();
        public override ZoneTypeSettings Parent { get; set; }
    }
    public class ZoneTypeSettingsLoader : ParentSettingsLoader<ZoneTypeSettings, ZoneType> { }

    public class ZoneTypeSettingsMapper : ParentSettingsMapper<ZoneTypeSettings, ZoneType, ZoneTypeSettingsDto> { }

}


