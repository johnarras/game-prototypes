using Genrpg.Shared.CoreCurrencies.Constants;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using MessagePack;

namespace Genrpg.Shared.CoreCurrencies.Settings
{
    [MessagePackObject]
    public class CoreCurrencyTypeSettings : ParentConstantListSettings<CoreCurrencyType, CoreCurrencyTypes>
    {
        [Key(0)] public override string Id { get; set; }


        public string GetName(long CoreCurrencyTypeId)
        {
            return Get(CoreCurrencyTypeId)?.Name ?? "Unknown";
        }
    }
    [MessagePackObject]
    public class CoreCurrencyType : ChildSettings, IIndexedGameItem
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
        [Key(9)] public long RegenTraderStatId { get; set; }
        [Key(10)] public long StorageTraderStatId { get; set; }
    }
    public class CoreCurrencyTypeSettingsDto : ParentSettingsDto<CoreCurrencyTypeSettings, CoreCurrencyType> { }
    public class CoreCurrencyTypeSettingsLoader : ParentSettingsLoader<CoreCurrencyTypeSettings, CoreCurrencyType> { }

    public class CoreCurrencyTypeSettingsMapper : ParentSettingsMapper<CoreCurrencyTypeSettings, CoreCurrencyType, CoreCurrencyTypeSettingsDto> { }

    public class CoreCurrencyTypeHelper : BaseEntityHelper<CoreCurrencyTypeSettings, CoreCurrencyType>
    {
        public override long HelperKey => EntityTypes.CoreCurrency;
    }
}
