using Genrpg.Shared.Interfaces;
using Genrpg.Shared.TimedEvents.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.TimedEvents.Interfaces
{
    public interface ITimedEventTheme : IIdName
    {
        List<TimedEventCustomReward> CustomRewards { get; set; }
    }
}
