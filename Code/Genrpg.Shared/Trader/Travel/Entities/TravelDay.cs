using Genrpg.Shared.Client.Interfaces;
using Genrpg.Shared.LevelTracks.WebApi;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Trader.Encounters.Entities;
using System.Collections.Generic;

namespace Genrpg.Shared.Trader.Travel.Entities
{
    public class TravelDay : IClientEvent
    {
        public List<int> RolledDistances { get; set; } = new List<int>();
        public int BonusDistance { get; set; }
        public int TotalDistance { get; set; }
        public int EndDistance { get; set; }
        public int Day { get; set; }
        public int EndFlags { get; set; }
        public int EndDiceSpeed { get; set; }
        public int EndBonusSpeed { get; set; }
        public int RationsCost { get; set; }
        public int DebuffDaysAdded { get; set; }
        public List<Reward> TravelRewards { get; set; } = new List<Reward>();

        public EncounterResult EncounterResult { get; set; } = null!;
        public GainExpResponse ExpResponse { get; set; } = null!;
    }
}
