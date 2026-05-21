using OxDb.SharedCore.Website.Interfaces;
using OxDb.SharedGame.Sexes.Constants;

namespace OxDb.SharedGame.Characters.WebApi.CreateChar
{
    public class CreateCharRequest : IClientUserRequest
    {
        public string Name { get; set; }
        public long UnitTypeId { get; set; } = 1;
        public long SexTypeId { get; set; } = SexTypes.Male;
    }
}


