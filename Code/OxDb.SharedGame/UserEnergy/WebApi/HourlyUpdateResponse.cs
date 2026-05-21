using OxDb.SharedCore.Rewards.Entities;
using OxDb.SharedCore.Website.Responses.Interfaces;
using System;
using System.Collections.Generic;

namespace OxDb.SharedGame.UserEnergy.WebApi
{
    public class HourlyUpdateResponse : IWebResponse
    {
        public List<Reward> Rewards { get; set; } = new List<Reward>();
        public DateTime NextHourlyUpdate { get; set; }
        public int Day { get; set; }
    }

}


