using Genrpg.Shared.Website.Interfaces;

namespace Genrpg.Shared.Trader.Animals.WebApi
{
    public class AddAnimalToCaravanRequest : IClientUserRequest
    {
        public long AnimalTypeId { get; set; }
    }
}
