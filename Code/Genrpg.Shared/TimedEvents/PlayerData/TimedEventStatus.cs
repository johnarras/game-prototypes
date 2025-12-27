using MessagePack;
using Genrpg.Shared.Characters.PlayerData;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.TimedEvents.PlayerData
{
    public class TimedEventStatus
    {
        public long TimedEventTypeId { get; set; }
        public string UniqueId { get; set; }
        public long Points { get; set; }
        public int CollectedTier { get; set; }
        public int CurrentTier { get; set; }
        public DateTime EndDate { get; set; }
    }
}


