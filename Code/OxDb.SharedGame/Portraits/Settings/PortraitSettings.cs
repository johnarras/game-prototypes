using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;

namespace OxDb.SharedGame.Portraits.Settings
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


