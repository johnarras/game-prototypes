using MessagePack;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;

namespace Genrpg.Shared.Crawler.Stats.Settings
{

    public class CrawlerStatSettings : NoChildSettings // No List
    {
        public override string Id { get; set; }
        public int MinStartValue { get; set; }
        public int MaxStartValue { get; set; }
        // Set because the secondary buff stats are compared using ratios of sizes and we don't want lowlevel combat
        // vs +1 level monsters to instakill the party.
        public int BaseBuffStatValue { get; set; }
        public double BonusScalingMult { get; set; }
        public double BonusScalingPower { get; set; }
        public double BonusScalingStartVal { get; set; }
    }


    public class CrawlerStatSettingsLoader : NoChildSettingsLoader<CrawlerStatSettings> { }


    public class CrawlerStatSettingsDto : NoChildSettingsDto<CrawlerStatSettings>
    {
        public override CrawlerStatSettings Parent { get; set; }
        public override string Id { get; set; }
    }

    public class CrawlerStatSettingsMapper : NoChildSettingsMapper<CrawlerStatSettings, CrawlerStatSettingsDto> { }
}


