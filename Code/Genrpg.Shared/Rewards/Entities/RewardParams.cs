using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Rewards.Entities
{
    [MessagePackObject]
    public class RewardParams
    {
       [Key(0)] public bool SkipVisualUpdate { get; set; }
    }
}
