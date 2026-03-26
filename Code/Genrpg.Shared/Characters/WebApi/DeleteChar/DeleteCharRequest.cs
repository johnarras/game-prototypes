using Genrpg.Shared.Website.Interfaces;

namespace Genrpg.Shared.Characters.WebApi.DeleteChar
{
    public class DeleteCharRequest : IClientUserRequest
    {
        public string CharId { get; set; }
    }
}


