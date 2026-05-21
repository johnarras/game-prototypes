
using OxDb.SharedCore.Rewards.Entities;
using System.Collections.Generic;

namespace OxDb.SharedGame.Trader.Encounters.Entities
{
    public class EncounterResult
    {
        public bool IsBad { get; set; }
        public List<RewardList> RewardLists { get; set; } = new List<RewardList>();

        public string Message { get; set; }

        public bool DidFail { get; set; }
    }
}
