using MessagePack;
using Genrpg.Shared.Crawler.Currencies.Constants;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using System.Collections.Generic;

namespace Genrpg.Shared.Crawler.Currencies.Settings
{
    public class CrawlerCurrencySettings : ParentConstantListSettings<CrawlerCurrencyType, CrawlerCurrencyTypes>
    {
        public override string Id { get; set; }
    }

    public class CrawlerCurrencyType : ChildSettings, IIndexedGameItem
    {

        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string PluralName { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }
        public long CraftingStatTypeId { get; set; }

    }

    public class CrawlerCurrencySettingsDto : ParentSettingsDto<CrawlerCurrencySettings, CrawlerCurrencyType>
    {
        public override List<CrawlerCurrencyType> Children { get; set; }
        public override CrawlerCurrencySettings Parent { get; set; }
        public override string Id { get; set; }
    }

    public class CrawlerCurrencySettingsLoader : ParentSettingsLoader<CrawlerCurrencySettings, CrawlerCurrencyType> { }

    public class CrawlerCurrencySettingsMapper : ParentSettingsMapper<CrawlerCurrencySettings, CrawlerCurrencyType, CrawlerCurrencySettingsDto> { }


    public class CrawlerCurrencyHelper : BaseEntityHelper<CrawlerCurrencySettings, CrawlerCurrencyType>
    {
        public override long HelperKey => EntityTypes.CrawlerCurrency;
    }

}


