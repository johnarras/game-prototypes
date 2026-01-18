using Genrpg.Shared.Website.Interfaces;

namespace Genrpg.Shared.Trader.Travel.WebApi
{
    public class HeadToTargetResponse : IWebResponse
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public long FromX { get; set; }
        public long FromY { get; set; }
        public long ToX { get; set; }
        public long ToY { get; set; }
        public long ToCityId { get; set; }
        public long DistanceToTarget { get; set; }
        public long NewTraderFlags { get; set; }
    }
}
