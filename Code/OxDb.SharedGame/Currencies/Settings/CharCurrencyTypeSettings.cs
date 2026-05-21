using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Entities.Helpers;
using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.GameSettings.Loaders;
using OxDb.SharedCore.GameSettings.Mappers;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.Currencies.Constants;
using System.Collections.Generic;

namespace OxDb.SharedGame.Currencies.Settings
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


