using MessagePack;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.TimedEvents.Constants;
using System.Collections.Generic;

namespace Genrpg.Shared.TimedEvents.Settings
{
    public class TimedEventCurrencySettings : ParentConstantListSettings<TimedEventCurrencyType, TimedEventCurrencyTypes>
    {
        public override string Id { get; set; }


        public string GetName(long TimedEventCurrencyTypeId)
        {
            return Get(TimedEventCurrencyTypeId)?.Name ?? "Unknown";
        }
    }
    public class TimedEventCurrencyType : ChildSettings, IIndexedGameItem
    {
        public const int None = 0;
        public const int Doubloons = 1;


        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string PluralName { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }
        public int BaseStorage { get; set; }

    }
    public class TimedEventCurrencySettingsDto : ParentSettingsDto<TimedEventCurrencySettings, TimedEventCurrencyType>
    {
        public override List<TimedEventCurrencyType> Children { get; set; }
        public override TimedEventCurrencySettings Parent { get; set; }
        public override string Id { get; set; }
    }
    public class UnitCoinSettingsLoader : ParentSettingsLoader<TimedEventCurrencySettings, TimedEventCurrencyType> { }

    public class TimedEventCurrencySettingsMapper : ParentSettingsMapper<TimedEventCurrencySettings, TimedEventCurrencyType, TimedEventCurrencySettingsDto> { }

    public class TimedEventCurrencyHelper : BaseEntityHelper<TimedEventCurrencySettings, TimedEventCurrencyType>
    {
        public override long HelperKey => EntityTypes.TimedEventCurrency;
    }
}


