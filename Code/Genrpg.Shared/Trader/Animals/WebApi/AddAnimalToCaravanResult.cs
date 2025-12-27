using Genrpg.Shared.Trader.Caravans.Entities;
using Genrpg.Shared.Website.Interfaces;

namespace Genrpg.Shared.Trader.Animals.WebApi
{
    public class AddAnimalToCaravanResult : IWebResponse
    {
        public long AnimalTypeId { get; set; }
        public long SkinTypeId { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public CaravanTravelInfo Travel { get; set; }
    }
}


