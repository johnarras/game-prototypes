using OxDb.SharedCore.Website.Interfaces;

namespace OxDb.SharedGame.Trader.Travel.WebApi
{
    public class HeadToTargetRequest : IClientUserRequest
    {
        public int ToX { get; set; }
        public int ToY { get; set; }
        public int ToCityId { get; set; }
    }
}
