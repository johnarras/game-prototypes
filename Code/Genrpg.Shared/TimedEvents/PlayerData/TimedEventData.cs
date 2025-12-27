using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Genrpg.Shared.TimedEvents.PlayerData
{
    public class TimedEventData
    {
        public List<TimedEventStatus> Data { get; set; } = new List<TimedEventStatus>();

        public TimedEventStatus GetStatus(long timedEventTypeId)
        {
            TimedEventStatus status = Data.FirstOrDefault(x=>x.TimedEventTypeId == timedEventTypeId);   
            if (status ==null)
            {
                status = new TimedEventStatus()
                {
                    TimedEventTypeId = timedEventTypeId,
                };
                Data.Add(status);
            }
            return status;
        }
    }
}


