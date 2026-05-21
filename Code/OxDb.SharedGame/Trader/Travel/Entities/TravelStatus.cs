using OxDb.SharedGame.Trader.Caravans.Entities;
using OxDb.SharedGame.Trader.Travel.WebApi;

namespace OxDb.SharedGame.Trader.Travel.Entities
{
    public class TravelStatus
    {
        public int DistanceGone { get; set; }
        public int TotalDistanceTravelledToday { get; set; }
        public int TotalDistanceToTarget { get; set; }
        public int TargetCityId { get; set; }
        public bool ArrivedInCity { get; set; }
        public int TravelDays { get; set; }
        public bool EarlyStopMessage { get; set; }
        public TravelResponse Response { get; set; }
        public CaravanTravelInfo TravelInfo { get; set; }
    }
}
