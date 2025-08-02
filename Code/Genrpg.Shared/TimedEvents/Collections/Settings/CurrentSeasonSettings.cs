using MessagePack;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.TimedEvents.Constants;
using Genrpg.Shared.TimedEvents.Interfaces;
using System;

namespace Genrpg.Shared.TimedEvents.Collections.Settings
{
    [MessagePackObject]
    public class CurrentCollectionSettings : NoChildSettings, ICurrentTimedEventSettings
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public DateTime StarTime { get; set; }
        [Key(2)] public DateTime EndTime { get; set; }
        [Key(3)] public bool Enabled { get; set; }
        [Key(4)] public long CollectionThemeId { get; set; }
        [Key(5)] public long CollectionTierListId { get; set; }
        [Key(6)] public string InstanceId { get; set; }

        public long GetActivityTypeId() { return TimedEventTypes.Collection; }
        public long GetThemeEntityTypeId() { return EntityTypes.CollectionTheme; }
        public long GetTierListEntityTypeId() { return EntityTypes.CollectionTierList; }

        public long GetThemeId() { return CollectionThemeId; }
        public long GetTierListId() { return CollectionTierListId; }


    }
}
