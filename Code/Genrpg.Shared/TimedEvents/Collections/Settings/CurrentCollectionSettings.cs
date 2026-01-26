using MessagePack;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.TimedEvents.Constants;
using Genrpg.Shared.TimedEvents.Interfaces;
using System;

namespace Genrpg.Shared.TimedEvents.Collections.Settings
{
    public class CurrentCollectionSettings : NoChildSettings, ICurrentTimedEventSettings
    {
        public override string Id { get; set; }
        public DateTime StarTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool Enabled { get; set; }
        public long CollectionThemeId { get; set; }
        public long CollectionTierListId { get; set; }
        public string InstanceId { get; set; }

        public long GetActivityTypeId() { return TimedEventTypes.Collection; }
        public long GetThemeEntityTypeId() { return EntityTypes.CollectionTheme; }
        public long GetTierListEntityTypeId() { return EntityTypes.CollectionTierList; }

        public long GetThemeId() { return CollectionThemeId; }
        public long GetTierListId() { return CollectionTierListId; }


    }
}


