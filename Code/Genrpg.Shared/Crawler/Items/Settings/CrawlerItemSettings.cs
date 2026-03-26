using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;

namespace Genrpg.Shared.Crawler.Items.Settings
{
    public class CrawlerItemSettings : NoChildSettings // No List
    {
        public override string Id { get; set; }
        public int MaxUsesBetweenCombats { get; set; }
    }


    public class CrawlerItemSettingsLoader : NoChildSettingsLoader<CrawlerItemSettings> { }


    public class CrawlerItemSettingsDto : NoChildSettingsDto<CrawlerItemSettings>
    {
        public override CrawlerItemSettings Parent { get; set; }
        public override string Id { get; set; }
    }

    public class CrawlerItemSettingsMapper : NoChildSettingsMapper<CrawlerItemSettings, CrawlerItemSettingsDto> { }
}


