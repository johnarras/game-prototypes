using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Purchasing.Settings;
using Genrpg.Shared.Zones.Constants;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Zones.Settings
{
    [MessagePackObject]
    public class ZoneTypeSettings : ParentConstantListSettings<ZoneType,ZoneTypes>
    {
        [Key(0)] public override string Id { get; set; }
    }

    public class ZoneTypeSettingsDto : ParentSettingsDto<ZoneTypeSettings, ZoneType> { }
    public class ZoneTypeSettingsLoader : ParentSettingsLoader<ZoneTypeSettings, ZoneType> { }

    public class ZoneTypeSettingsMapper : ParentSettingsMapper<ZoneTypeSettings, ZoneType, ZoneTypeSettingsDto> { }

}
