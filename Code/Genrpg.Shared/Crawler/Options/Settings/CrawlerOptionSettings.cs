using Genrpg.Shared.Crawler.Options.Constants;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using MessagePack;

namespace Genrpg.Shared.Crawler.Options.Settings
{
    [MessagePackObject]
    public class CrawlerOptionSettings : ParentConstantListSettings<CrawlerOption, CrawlerOptions>
    {
        [Key(0)] public override string Id { get; set; }
    }

    [MessagePackObject]
    public class CrawlerOption : ChildSettings, IIndexedGameItem
    {

        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string AtlasPrefix { get; set; }
        [Key(6)] public string Icon { get; set; }
        [Key(7)] public string Art { get; set; }
        [Key(8)] public bool DefaultForNewGame { get; set; }

    }


    public class CrawlerOptionSettingsDto : ParentSettingsDto<CrawlerOptionSettings, CrawlerOption> { }
    public class CrawlerOptionSettingsLoader : ParentSettingsLoader<CrawlerOptionSettings, CrawlerOption> { }

    public class CrawlerOptionSettingsMapper : ParentSettingsMapper<CrawlerOptionSettings, CrawlerOption, CrawlerOptionSettingsDto> { }

}
