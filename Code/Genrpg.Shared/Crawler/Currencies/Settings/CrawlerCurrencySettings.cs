using Genrpg.Shared.Crawler.Currencies.Constants;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using MessagePack;

namespace Genrpg.Shared.Crawler.Currencies.Settings
{
    [MessagePackObject]
    public class CrawlerCurrencySettings : ParentConstantListSettings<CrawlerCurrencyType, CrawlerCurrencyTypes>
    {
        [Key(0)] public override string Id { get; set; }
    }

    [MessagePackObject]
    public class CrawlerCurrencyType : ChildSettings, IIndexedGameItem
    {

        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string PluralName { get; set; }
        [Key(5)] public string Desc { get; set; }
        [Key(6)] public string AtlasPrefix { get; set; }
        [Key(7)] public string Icon { get; set; }
        [Key(8)] public string Art { get; set; }

    }

    public class CrawlerCurrencySettingsDto : ParentSettingsDto<CrawlerCurrencySettings, CrawlerCurrencyType> { }

    public class CrawlerCurrencySettingsLoader : ParentSettingsLoader<CrawlerCurrencySettings, CrawlerCurrencyType> { }

    public class CrawlerCurrencySettingsMapper : ParentSettingsMapper<CrawlerCurrencySettings, CrawlerCurrencyType, CrawlerCurrencySettingsDto> { }


    public class CrawlerCurrencyHelper : BaseEntityHelper<CrawlerCurrencySettings, CrawlerCurrencyType>
    {
        public override long Key => EntityTypes.CrawlerCurrency;
    }

}
