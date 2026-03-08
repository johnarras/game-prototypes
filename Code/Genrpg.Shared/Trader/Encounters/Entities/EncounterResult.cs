using Genrpg.Shared.Rewards.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Trader.Encounters.Entities
{
    public class EncounterResult
    {
        public bool IsBad { get; set; }
        public List<RewardList> RewardLists { get; set; } = new List<RewardList>();

        public string Message { get; set; }

        public bool DidFail { get; set; }
    }
}
