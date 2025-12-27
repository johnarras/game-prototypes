using Genrpg.Shared.Website.Interfaces;

namespace Genrpg.Shared.Trader.Roads.WebApi
{
    public class TurnAroundResponse : IWebResponse
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public long RoadId { get; set; }
        public long TargetCityId { get; set; }
        public long DistanceTravelled { get; set; }
    }
}
