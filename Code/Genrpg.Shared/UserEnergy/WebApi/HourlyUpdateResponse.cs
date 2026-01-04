using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Website.Interfaces;
using System;
using System.Collections.Generic;

namespace Genrpg.Shared.UserEnergy.WebApi
{
    public class HourlyUpdateResponse : IWebResponse
    {
        public List<Reward> Rewards { get; set; } = new List<Reward>();
        public DateTime NextHourlyUpdate { get; set; }
        public long Day { get; set; }
    }

}


