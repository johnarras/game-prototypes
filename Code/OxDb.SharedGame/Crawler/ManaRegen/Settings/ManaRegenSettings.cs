using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;

namespace OxDb.SharedGame.Crawler.ManaRegen.Settings
{
    public class ManaRegenSettings : NoChildSettings // No List
    {
        public override string Id { get; set; }
        public long CostPerMana { get; set; }
    }


    public class ManaRegenSettingsLoader : NoChildSettingsLoader<ManaRegenSettings> { }


    public class ManaRegenSettingsDto : NoChildSettingsDto<ManaRegenSettings>
    {
        public override ManaRegenSettings Parent { get; set; }
        public override string Id { get; set; }
    }

    public class ManaRegenSettingsMapper : NoChildSettingsMapper<ManaRegenSettings, ManaRegenSettingsDto> { }
}


