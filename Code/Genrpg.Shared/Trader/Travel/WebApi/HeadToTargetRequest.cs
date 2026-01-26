using Genrpg.Shared.Website.Interfaces;

namespace Genrpg.Shared.Trader.Travel.WebApi
{
    public class HeadToTargetRequest : IClientUserRequest
    {
        public int ToX { get; set; }
        public int ToY { get; set; }
        public int ToCityId { get; set; }
    }
}
