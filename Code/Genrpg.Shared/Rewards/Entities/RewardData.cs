using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Rewards.Entities
{
    public class RewardData
    {
        public List<RewardList> Rewards { get; set; } = new List<RewardList>();
    }
}


