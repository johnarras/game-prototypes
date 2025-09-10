using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using MessagePack;

namespace Genrpg.Shared.Crawler.Items.Settings
{
    [MessagePackObject]
    public class CrawlerItemSettings : NoChildSettings // No List
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public int MaxUsesBetweenCombats { get; set; }
    }


    public class CrawlerItemSettingsLoader : NoChildSettingsLoader<CrawlerItemSettings> { }


    public class CrawlerItemSettingsDto : NoChildSettingsDto<CrawlerItemSettings> { }

    public class CrawlerItemSettingsMapper : NoChildSettingsMapper<CrawlerItemSettings, CrawlerItemSettingsDto> { }
}
