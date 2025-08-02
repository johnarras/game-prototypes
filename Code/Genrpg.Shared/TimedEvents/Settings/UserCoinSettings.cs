using MessagePack;

using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;
using Genrpg.Shared.TimedEvents.Constants;

namespace Genrpg.Shared.TimedEvents.Settings
{
    [MessagePackObject]
    public class TimedEventCurrencySettings : ParentConstantListSettings<TimedEventCurrencyType, TimedEventCurrencyTypes>
    {
        [Key(0)] public override string Id { get; set; }


        public string GetName(long TimedEventCurrencyTypeId)
        {
            return Get(TimedEventCurrencyTypeId)?.Name ?? "Unknown";
        }
    }
    [MessagePackObject]
    public class TimedEventCurrencyType : ChildSettings, IIndexedGameItem
    {
        public const int None = 0;
        public const int Doubloons = 1;


        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string PluralName { get; set; }
        [Key(5)] public string Desc { get; set; }
        [Key(6)] public string AtlasPrefix { get; set; }
        [Key(7)] public string Icon { get; set; }
        [Key(8)] public string Art { get; set; }
        [Key(9)] public int BaseStorage { get; set; }

    }
    public class TimedEventCurrencySettingsDto : ParentSettingsDto<TimedEventCurrencySettings, TimedEventCurrencyType> { }
    public class UnitCoinSettingsLoader : ParentSettingsLoader<TimedEventCurrencySettings, TimedEventCurrencyType> { }

    public class TimedEventCurrencySettingsMapper : ParentSettingsMapper<TimedEventCurrencySettings, TimedEventCurrencyType, TimedEventCurrencySettingsDto> { }

    public class TimedEventCurrencyHelper : BaseEntityHelper<TimedEventCurrencySettings, TimedEventCurrencyType>
    {
        public override long Key => EntityTypes.TimedEventCurrency;
    }
}
