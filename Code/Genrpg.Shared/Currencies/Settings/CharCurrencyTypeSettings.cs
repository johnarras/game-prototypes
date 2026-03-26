using Genrpg.Shared.Currencies.Constants;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using System.Collections.Generic;

namespace Genrpg.Shared.Currencies.Settings
{
    public class CharCurrencyTypeSettings : ParentConstantListSettings<CharCurrencyType, CharCurrencyTypes>
    {
        public override string Id { get; set; }
    }

    public class CharCurrencyType : ChildSettings, IIndexedGameItem
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

    public class CharCurrencySettingsDto : ParentSettingsDto<CharCurrencyTypeSettings, CharCurrencyType>
    {
        public override List<CharCurrencyType> Children { get; set; }
        public override CharCurrencyTypeSettings Parent { get; set; }
        public override string Id { get; set; }
    }

    public class CharCurrencySettingsLoader : ParentSettingsLoader<CharCurrencyTypeSettings, CharCurrencyType> { }

    public class CharCurrencySettingsMapper : ParentSettingsMapper<CharCurrencyTypeSettings, CharCurrencyType, CharCurrencySettingsDto> { }


    public class CharCurrencyHelper : BaseEntityHelper<CharCurrencyTypeSettings, CharCurrencyType>
    {
        public override long HelperKey => EntityTypes.CharCurrency;
    }

}


