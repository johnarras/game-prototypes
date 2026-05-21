using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.Crawler.Options.Constants;
using System.Collections.Generic;

namespace OxDb.SharedGame.Crawler.Options.Settings
{
    public class CrawlerOptionSettings : ParentConstantListSettings<CrawlerOption, CrawlerOptions>
    {
        public override string Id { get; set; }
    }

    public class CrawlerOption : ChildSettings, IIndexedGameItem
    {

        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }
        public bool DefaultForNewGame { get; set; }

    }


    public class CrawlerOptionSettingsDto : ParentSettingsDto<CrawlerOptionSettings, CrawlerOption>
    {
        public override List<CrawlerOption> Children { get; set; }
        public override CrawlerOptionSettings Parent { get; set; }
        public override string Id { get; set; }
    }
    public class CrawlerOptionSettingsLoader : ParentSettingsLoader<CrawlerOptionSettings, CrawlerOption> { }

    public class CrawlerOptionSettingsMapper : ParentSettingsMapper<CrawlerOptionSettings, CrawlerOption, CrawlerOptionSettingsDto> { }

}


