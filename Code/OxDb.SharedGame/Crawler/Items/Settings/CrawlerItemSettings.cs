using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;

namespace OxDb.SharedGame.Crawler.Items.Settings
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


