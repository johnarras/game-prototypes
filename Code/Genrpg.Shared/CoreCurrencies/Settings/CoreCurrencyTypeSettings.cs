using MessagePack;
using Genrpg.Shared.CoreCurrencies.Constants;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using System.Collections.Generic;

namespace Genrpg.Shared.CoreCurrencies.Settings
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
        public long RegenTraderStatId { get; set; }
        public long StorageTraderStatId { get; set; }
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
}


