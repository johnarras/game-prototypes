using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Website.Interfaces;
using MessagePack;
using System;
using System.Collections.Generic;

namespace Genrpg.Shared.UserEnergy.WebApi
{
    public class UpdateCoreCurrenciesResponse : IWebResponse
    {
        public List<Reward> Rewards { get; set; } = new List<Reward>();
        public DateTime NextHourlyUpdate { get; set; }
    }

}


