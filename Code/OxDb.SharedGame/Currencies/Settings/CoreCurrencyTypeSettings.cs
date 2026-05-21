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
    public class CoreCurrencyTypeSettings : ParentConstantListSettings<CoreCurrencyType, CoreCurrencyTypes>
    {
        public override string Id { get; set; }


        public string GetName(long CoreCurrencyTypeId)
        {
            return Get(CoreCurrencyTypeId)?.Name ?? "Unknown";
        }
    }
    public class CoreCurrencyType : ChildSettings, IIndexedGameItem
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

        public long StatTypeId { get; set; }
    }
    public class CoreCurrencyTypeSettingsDto : ParentSettingsDto<CoreCurrencyTypeSettings, CoreCurrencyType>
    {
        public override List<CoreCurrencyType> Children { get; set; }
        public override CoreCurrencyTypeSettings Parent { get; set; }
        public override string Id { get; set; }
    }
    public class CoreCurrencyTypeSettingsLoader : ParentSettingsLoader<CoreCurrencyTypeSettings, CoreCurrencyType> { }

    public class CoreCurrencyTypeSettingsMapper : ParentSettingsMapper<CoreCurrencyTypeSettings, CoreCurrencyType, CoreCurrencyTypeSettingsDto> { }

    public class CoreCurrencyTypeHelper : BaseEntityHelper<CoreCurrencyTypeSettings, CoreCurrencyType>
    {
        public override long HelperKey => EntityTypes.CoreCurrency;
    }

    public class BaseCurrencyRegenEntityHelper : BaseEntityHelper<CoreCurrencyTypeSettings, CoreCurrencyType>
    {
        public override long HelperKey => EntityTypes.BaseCurrencyRegen;
    }

    public class BonusCurrencyRegenEntityHelper : BaseEntityHelper<CoreCurrencyTypeSettings, CoreCurrencyType>
    {
        public override long HelperKey => EntityTypes.BonusCurrencyRegen;
    }


    public class CurrencyRegenBuffEntityHelper : BaseEntityHelper<CoreCurrencyTypeSettings, CoreCurrencyType>
    {
        public override long HelperKey => EntityTypes.CurrencyRegenBuff;
    }


    public class BaseCurrencyStorageEntityHelper : BaseEntityHelper<CoreCurrencyTypeSettings, CoreCurrencyType>
    {
        public override long HelperKey => EntityTypes.BaseCurrencyStorage;
    }

    public class BonusCurrencyStorageEntityHelper : BaseEntityHelper<CoreCurrencyTypeSettings, CoreCurrencyType>
    {
        public override long HelperKey => EntityTypes.BonusCurrencyStorage;
    }

    public class CurrencyStorageBuffEntityHelper : BaseEntityHelper<CoreCurrencyTypeSettings, CoreCurrencyType>
    {
        public override long HelperKey => EntityTypes.CurrencyStorageBuff;
    }


    public class BaseTravelDayCurrencyEntityHelper : BaseEntityHelper<CoreCurrencyTypeSettings, CoreCurrencyType>
    {
        public override long HelperKey => EntityTypes.BaseTravelDayCurrency;
    }

    public class BonusTravelDayCurrencyEntityHelper : BaseEntityHelper<CoreCurrencyTypeSettings, CoreCurrencyType>
    {
        public override long HelperKey => EntityTypes.BonusTravelDayCurrency;
    }

    public class TravelDayCurrencyBuffEntityHelper : BaseEntityHelper<CoreCurrencyTypeSettings, CoreCurrencyType>
    {
        public override long HelperKey => EntityTypes.TravelDayCurrencyBuff;
    }


}


