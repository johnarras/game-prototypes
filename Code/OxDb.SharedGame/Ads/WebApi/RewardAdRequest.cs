using OxDb.SharedCore.Website.Requests.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace OxDb.SharedGame.Ads.WebApi
{
    public class RewardAdRequest : IWebRequest
    {
        public string AdUnitId { get; set; }      
    }
}
