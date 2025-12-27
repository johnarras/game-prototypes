using MessagePack;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using System.Collections.Generic;

namespace Genrpg.Shared.TimedEvents.Settings
{
    public class TimedEventTypeSettings : ParentSettings<TimedEventType>
    {
        public override string Id { get; set; }
    }

    public class TimedEventType : ChildSettings, IIndexedGameItem
    {

        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }

    }

    public class TimedEventSettingsDto : ParentSettingsDto<TimedEventTypeSettings, TimedEventType>
    {
        public override List<TimedEventType> Children { get; set; }
        public override TimedEventTypeSettings Parent { get; set; }
        public override string Id { get; set; }
    }

    public class TimedEventSettingsLoader : ParentSettingsLoader<TimedEventTypeSettings, TimedEventType> { }

    public class TimedEventSettingsMapper : ParentSettingsMapper<TimedEventTypeSettings, TimedEventType, TimedEventSettingsDto> { }


    public class TimedEventHelper : BaseEntityHelper<TimedEventTypeSettings, TimedEventType>
    {
        public override long HelperKey => EntityTypes.TimedEvent;
    }

}


