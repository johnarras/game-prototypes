using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Website.Interfaces;
using MessagePack;
using System;
using System.Collections.Generic;

namespace Genrpg.Shared.UserEnergy.WebApi
{
    [MessagePackObject]
    public class UpdateCoreCurrenciesResponse : IWebResponse
    {
        [Key(0)] public List<Reward> Rewards { get; set; } = new List<Reward>();
        [Key(1)] public DateTime NextHourlyUpdate { get; set; }
    }

}
