using MessagePack;
using Genrpg.Shared.Characters.PlayerData;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.TimedEvents.PlayerData
{
    [MessagePackObject]
    public class TimedEventStatus
    {
        [Key(0)] public long TimedEventTypeId { get; set; }
        [Key(1)] public string UniqueId { get; set; }
        [Key(2)] public long Points { get; set; }
        [Key(3)] public int CollectedTier { get; set; }
        [Key(4)] public int CurrentTier { get; set; }
        [Key(5)] public DateTime EndDate { get; set; }
    }
}
