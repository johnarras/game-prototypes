using OxDb.SharedCore.Rewards.Entities;
using OxDb.SharedCore.Website.Responses.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace OxDb.SharedGame.Ads.WebApi
{
    public class RewardAdResponse : IWebResponse
    {
        public RewardData Rewards { get; set; }
        public int AdsSeenToday { get; set; }
    }
}
