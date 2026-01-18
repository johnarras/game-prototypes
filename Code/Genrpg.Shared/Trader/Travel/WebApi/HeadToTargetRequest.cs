using Genrpg.Shared.Website.Interfaces;

namespace Genrpg.Shared.Trader.Travel.WebApi
{
    public class HeadToTargetRequest : IClientUserRequest
    {
        public long ToX { get; set; }
        public long ToY { get; set; }
        public long ToCityId { get; set; }
    }
}
