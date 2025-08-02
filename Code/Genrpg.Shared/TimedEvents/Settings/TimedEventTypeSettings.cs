using MessagePack;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;

namespace Genrpg.Shared.TimedEvents.Settings
{
    [MessagePackObject]
    public class TimedEventTypeSettings : ParentSettings<TimedEventType>
    {
        [Key(0)] public override string Id { get; set; }
    }

    [MessagePackObject]
    public class TimedEventType : ChildSettings, IIndexedGameItem
    {

        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string AtlasPrefix { get; set; }
        [Key(6)] public string Icon { get; set; }
        [Key(7)] public string Art { get; set; }

    }

    public class TimedEventSettingsDto : ParentSettingsDto<TimedEventTypeSettings, TimedEventType> { }

    public class TimedEventSettingsLoader : ParentSettingsLoader<TimedEventTypeSettings, TimedEventType> { }

    public class TimedEventSettingsMapper : ParentSettingsMapper<TimedEventTypeSettings, TimedEventType, TimedEventSettingsDto> { }


    public class TimedEventHelper : BaseEntityHelper<TimedEventTypeSettings, TimedEventType>
    {
        public override long Key => EntityTypes.TimedEvent;
    }

}
