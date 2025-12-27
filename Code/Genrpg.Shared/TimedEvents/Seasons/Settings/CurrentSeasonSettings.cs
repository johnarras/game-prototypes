using MessagePack;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.TimedEvents.Constants;
using Genrpg.Shared.TimedEvents.Interfaces;
using System;

namespace Genrpg.Shared.TimedEvents.Seasons.Settings
{
    public class CurrentSeasonSettings : NoChildSettings, ICurrentTimedEventSettings
    {
        public override string Id { get; set; }
        public DateTime StarTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool Enabled { get; set; }
        public long SeasonThemeId { get; set; }
        public long SeasonTierListId { get; set; }
        public string InstanceId { get; set; }

        public long GetActivityTypeId() { return TimedEventTypes.Season; }
        public long GetThemeEntityTypeId() { return EntityTypes.SeasonTheme; }
        public long GetTierListEntityTypeId() { return EntityTypes.SeasonTierList; }

        public long GetThemeId() { return SeasonThemeId; }
        public long GetTierListId() { return SeasonTierListId; }


    }
}


