using Genrpg.Shared.Website.Interfaces;

namespace Genrpg.Shared.Trader.Cities.WebApi
{
    public class EnterCityRequest : IClientUserRequest
    {
        public long CityId { get; set; }
    }
}
