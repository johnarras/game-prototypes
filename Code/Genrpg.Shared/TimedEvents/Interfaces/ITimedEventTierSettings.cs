using System.Collections.Generic;

namespace Genrpg.Shared.TimedEvents.Interfaces
{
    public interface ITimedEventTierSettings 
    {
        ITimedEventTierList GetTierList(long id);
    }
}
