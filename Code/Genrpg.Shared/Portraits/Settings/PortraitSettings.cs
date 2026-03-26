using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;

namespace Genrpg.Shared.Portraits.Settings
{
    public class PortraitSettings : NoChildSettings
    {
        public override string Id { get; set; }
        public int PortraitCount { get; set; }
    }

    public class PortraitSettingsDto : NoChildSettingsDto<PortraitSettings>
    {
        public override PortraitSettings Parent { get; set; }
        public override string Id { get; set; }
    }
    public class PortraitSettingsLoader : NoChildSettingsLoader<PortraitSettings> { }

    public class PortraitSettingsMapper : NoChildSettingsMapper<PortraitSettings, PortraitSettingsDto> { }

}


