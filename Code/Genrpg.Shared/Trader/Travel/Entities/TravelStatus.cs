using Genrpg.Shared.Trader.Caravans.Entities;
using Genrpg.Shared.Trader.Travel.WebApi;

namespace Genrpg.Shared.Trader.Travel.Entities
{
    public class TravelStatus
    {
        public long CurrentDistanceAlongRoad { get; set; }
        public long TotalDistanceTravelled { get; set; }
        public long RoadDistance { get; set; }
        public long TargetCityId { get; set; }
        public bool ArrivedInCity { get; set; }
        public int TravelDays { get; set; }
        public bool IsFree { get; set; }
        public bool EarlyStopMessage { get; set; }
        public TravelResponse Response { get; set; }
        public CaravanTravelInfo TravelInfo { get; set; }
    }
}
