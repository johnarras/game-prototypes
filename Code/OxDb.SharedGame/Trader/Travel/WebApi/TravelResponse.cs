using OxDb.SharedCore.Website.Responses.Interfaces;
using OxDb.SharedGame.Trader.Travel.Entities;
using System.Collections.Generic;

namespace OxDb.SharedGame.Trader.Travel.WebApi
{
    public class TravelResponse : IWebResponse
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public int TargetCityId { get; set; }
        public int TotalDistanceTravelled { get; set; }
        public int DistanceLeft { get; set; }
        public int DistanceAlongRoad { get; set; }
        public int EndDay { get; set; }
        public int EnterCityId { get; set; }
        public List<TravelDay> Days { get; set; } = new List<TravelDay>();
        public List<string> Messages { get; set; } = new List<string>();
        public int EndCoreFlags { get; set; }
    }

}
