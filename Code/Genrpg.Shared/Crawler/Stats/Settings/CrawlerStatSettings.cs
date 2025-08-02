using Genrpg.Shared.BoardGame.Settings;
using Genrpg.Shared.Core.Settings;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using MessagePack;

namespace Genrpg.Shared.Crawler.Stats.Settings
{
    [MessagePackObject]
    public class CrawlerStatSettings : NoChildSettings // No List
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public int StartStat { get; set; }
        [Key(2)] public int MinRollValue { get; set; }
        [Key(3)] public int MaxRollValue { get; set; }
        // Set because the secondary buff stats are compared using ratios of sizes and we don't want lowlevel combat
        // vs +1 level monsters to instakill the party.
        [Key(4)] public int BaseBuffStatValue { get; set; } 
    }


    public class CrawlerStatSettingsLoader : NoChildSettingsLoader<CrawlerStatSettings> { }


    public class CrawlerStatSettingsDto : NoChildSettingsDto<CrawlerStatSettings> { }

    public class CrawlerStatSettingsMapper : NoChildSettingsMapper<CrawlerStatSettings, CrawlerStatSettingsDto> { }
}
