using Genrpg.Shared.Website.Interfaces;

namespace Genrpg.Shared.Trader.Roads.WebApi
{
    public class EnterRoadRequest : IClientUserRequest
    {
        public long RoadId { get; set; }
    }
}
