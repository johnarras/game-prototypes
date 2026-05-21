using OxDb.SharedCore.Website.Responses.Interfaces;

namespace OxDb.SharedGame.Trader.Travel.WebApi
{
    public class HeadToTargetResponse : IWebResponse
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public int FromX { get; set; }
        public int FromY { get; set; }
        public int ToX { get; set; }
        public int ToY { get; set; }
        public int ToCityId { get; set; }
        public int TotalDistanceToTarget { get; set; }
        public int NewTraderFlags { get; set; }
    }
}
