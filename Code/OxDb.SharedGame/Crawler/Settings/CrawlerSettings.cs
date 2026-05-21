using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;

namespace OxDb.SharedGame.Crawler.Settings
{
    public class CrawlerSettings : NoChildSettings // No List
    {
        public override string Id { get; set; }
        public long MaxPartySize { get; set; }
        public long StartGold { get; set; }
    }


    public class CrawlerSettingsLoader : NoChildSettingsLoader<CrawlerSettings> { }


    public class CrawlerSettingsDto : NoChildSettingsDto<CrawlerSettings>
    {
        public override CrawlerSettings Parent { get; set; }
        public override string Id { get; set; }
    }

    public class CrawlerSettingsMapper : NoChildSettingsMapper<CrawlerSettings, CrawlerSettingsDto> { }
}


