using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Zones.Constants;
using MessagePack;
using System.Collections.Generic;

namespace Genrpg.Shared.Zones.Settings
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


