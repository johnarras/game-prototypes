using System;

namespace Genrpg.Shared.TimedEvents.Interfaces
{
    public interface ICurrentTimedEventSettings
    {
        DateTime StarTime { get; set; }
        DateTime EndTime { get; set; }
        bool Enabled { get; set; }
        
        // Used to each new timed event has a unique id, can reuse theme, but this tells us when to reset player
        string InstanceId { get; set; } 

        long GetActivityTypeId();
        long GetThemeEntityTypeId();
        long GetThemeId();
        long GetTierListEntityTypeId();
        long GetTierListId();
    }
}
