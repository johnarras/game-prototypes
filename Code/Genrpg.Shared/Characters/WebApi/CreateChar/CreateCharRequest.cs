using MessagePack;
using Genrpg.Shared.Website.Interfaces;
using Genrpg.Shared.Sexes.Constants;

namespace Genrpg.Shared.Characters.WebApi.CreateChar
{
    public class CreateCharRequest : IClientUserRequest
    {
        public string Name { get; set; }
        public long UnitTypeId { get; set; } = 1;
        public long SexTypeId { get; set; } = SexTypes.Male;
    }
}


