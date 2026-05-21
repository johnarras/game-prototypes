using OxDb.SharedCore.Website.Interfaces;

namespace OxDb.SharedGame.Characters.WebApi.DeleteChar
{
    public class DeleteCharRequest : IClientUserRequest
    {
        public string CharId { get; set; }
    }
}


