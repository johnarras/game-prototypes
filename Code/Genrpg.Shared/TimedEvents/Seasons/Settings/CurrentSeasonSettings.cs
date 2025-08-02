using MessagePack;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.TimedEvents.Constants;
using Genrpg.Shared.TimedEvents.Interfaces;
using System;

namespace Genrpg.Shared.TimedEvents.Seasons.Settings
{
    [MessagePackObject]
    public class CurrentSeasonSettings : NoChildSettings, ICurrentTimedEventSettings
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public DateTime StarTime { get; set; }
        [Key(2)] public DateTime EndTime { get; set; }
        [Key(3)] public bool Enabled { get; set; }
        [Key(4)] public long SeasonThemeId { get; set; }
        [Key(5)] public long SeasonTierListId { get; set; }
        [Key(6)] public string InstanceId { get; set; }

        public long GetActivityTypeId() { return TimedEventTypes.Season; }
        public long GetThemeEntityTypeId() { return EntityTypes.SeasonTheme; }
        public long GetTierListEntityTypeId() { return EntityTypes.SeasonTierList; }

        public long GetThemeId() { return SeasonThemeId; }
        public long GetTierListId() { return SeasonTierListId; }


    }
}
