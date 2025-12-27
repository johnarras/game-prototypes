using MessagePack;
using Genrpg.Shared.Website.Interfaces;

namespace Genrpg.Shared.Trader.Cities.WebApi
{
    public class EnterCityResponse : IWebResponse
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public long CityId { get; set; }
    }
}
