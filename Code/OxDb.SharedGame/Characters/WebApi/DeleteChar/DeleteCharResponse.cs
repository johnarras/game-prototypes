using OxDb.SharedCore.Website.Responses.Interfaces;
using OxDb.SharedGame.Characters.PlayerData;
using System.Collections.Generic;

namespace OxDb.SharedGame.Characters.WebApi.DeleteChar
{
    public class DeleteCharResponse : IWebResponse
    {
        public List<CharacterStub> AllCharacters { get; set; }
    }
}


