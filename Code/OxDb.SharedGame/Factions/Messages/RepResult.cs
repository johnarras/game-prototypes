
using OxDb.SharedCore.Rewards.Entities;
using System.Collections.Generic;

namespace OxDb.SharedGame.Factions.Messages
{
    public class RepResult
    {
        public long FactionTypeId { get; set; }
        public long OldRepLevelId { get; set; }
        public long NewRepLevelId { get; set; }
        public long OldRep { get; set; }
        public long NewRep { get; set; }
        public long RepChange { get; set; }
        public List<Reward> Rewards { get; set; }
    }
}


