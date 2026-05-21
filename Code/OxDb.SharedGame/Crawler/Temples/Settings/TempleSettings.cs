using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;

namespace OxDb.SharedGame.Crawler.Temples.Settings
{
    public class TempleSettings : NoChildSettings // No List
    {
        public override string Id { get; set; }
        public long HealingCostPerLevel { get; set; } = 10;
        public long StatusEffectCostPerLevel { get; set; } = 100;
        public long MaxCostLevel { get; set; } = 25;
    }


    public class TempleSettingsLoader : NoChildSettingsLoader<TempleSettings> { }


    public class TempleSettingsDto : NoChildSettingsDto<TempleSettings>
    {
        public override TempleSettings Parent { get; set; }
        public override string Id { get; set; }
    }

    public class TempleSettingsMapper : NoChildSettingsMapper<TempleSettings, TempleSettingsDto> { }
}


