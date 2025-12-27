using MessagePack;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using System.Collections.Generic;

namespace Genrpg.Shared.Currencies.Settings
{
    public class CurrencySettings : ParentSettings<CurrencyType>
    {
        public override string Id { get; set; }
    }

    public class CurrencyType : ChildSettings, IIndexedGameItem
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

    }

    public class CurrencySettingsDto : ParentSettingsDto<CurrencySettings, CurrencyType>
    {
        public override List<CurrencyType> Children { get; set; }
        public override CurrencySettings Parent { get; set; }
        public override string Id { get; set; }
    }

    public class CurrencySettingsLoader : ParentSettingsLoader<CurrencySettings, CurrencyType> { }

    public class CurrencySettingsMapper : ParentSettingsMapper<CurrencySettings, CurrencyType, CurrencySettingsDto> { }


    public class CurrencyHelper : BaseEntityHelper<CurrencySettings, CurrencyType>
    {
        public override long HelperKey => EntityTypes.Currency;
    }

}


