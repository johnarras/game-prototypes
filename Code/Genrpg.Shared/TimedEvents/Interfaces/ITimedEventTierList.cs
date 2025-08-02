using Genrpg.Shared.TimedEvents.Entities;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.TimedEvents.Interfaces
{
    public interface ITimedEventTierList
    {
        List<TimedEventTier> Tiers { get; set; }
        int StartBonusPoints { get; set; }
        int BonusPointsPerTier { get; set; }
        long BonusEntityTypeId { get; set; }
        long BonusEntityId { get; set; }
        long BonusQuantity { get; set; }
    }
}
