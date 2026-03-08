using Genrpg.Shared.Trader.Travel.WebApi;

namespace Genrpg.Shared.Trader.Travel.Entities
{
    public class TravelStatus
    {
        public int DistanceGone { get; set; }
        public int TotalDistanceTravelledToday { get; set; }
        public int TotalDistanceToTarget { get; set; }
        public int TargetCityId { get; set; }
        public bool ArrivedInCity { get; set; }
        public int TravelDays { get; set; }
        public bool IsFree { get; set; }
        public bool EarlyStopMessage { get; set; }
        public TravelResponse Response { get; set; }
    }
}
