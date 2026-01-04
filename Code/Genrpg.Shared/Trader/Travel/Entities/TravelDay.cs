using Genrpg.Shared.Rewards.Entities;
using System.Collections.Generic;

namespace Genrpg.Shared.Trader.Travel.Entities
{
    public class TravelDay
    {
        public List<long> RolledDistances { get; set; } = new List<long>();
        public long BonusDistance { get; set; }
        public long TotalDistance { get; set; }
        public long EndDistance { get; set; }
        public long Day { get; set; }
        public List<Reward> TravelRewards { get; set; } = new List<Reward>();
    }
}
