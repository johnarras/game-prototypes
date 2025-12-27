using Genrpg.Shared.Trader.Caravans.Entities;
using Genrpg.Shared.Website.Interfaces;
namespace Genrpg.Shared.Trader.Animals.WebApi
{
    public class RemoveAnimalFromCaravanResult : IWebResponse
    {
        public long AnimalTypeId { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public CaravanTravelInfo Travel { get; set; }
    }
}


