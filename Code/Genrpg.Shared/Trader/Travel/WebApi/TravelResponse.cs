using Genrpg.Shared.Trader.Travel.Entities;
using Genrpg.Shared.Website.Interfaces;
using System.Collections.Generic;

namespace Genrpg.Shared.Trader.Travel.WebApi
{
    public class TravelResponse : IWebResponse
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public long RoadId { get; set; }
        public long TargetCityId { get; set; }
        public long TotalDistanceTravelled { get; set; }
        public long DistanceLeft { get; set; }
        public long TotalCost { get; set; }
        public long DistanceAlongRoad { get; set; }
        public long EndDay { get; set; }
        public List<TravelDay> Days { get; set; } = new List<TravelDay>();
        public List<string> Messages { get; set; } = new List<string>();
    }

}
