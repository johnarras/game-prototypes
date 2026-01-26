using Genrpg.Shared.Trader.Travel.Entities;
using Genrpg.Shared.Website.Interfaces;
using System.Collections.Generic;

namespace Genrpg.Shared.Trader.Travel.WebApi
{
    public class TravelResponse : IWebResponse
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public int TargetCityId { get; set; }
        public int TotalDistanceTravelled { get; set; }
        public int DistanceLeft { get; set; }
        public int TotalCost { get; set; }
        public int DistanceAlongRoad { get; set; }
        public int EndDay { get; set; }
        public int EnterCityId { get; set; }
        public List<TravelDay> Days { get; set; } = new List<TravelDay>();
        public List<string> Messages { get; set; } = new List<string>();
        public int EndCoreFlags { get; set; }
    }

}
