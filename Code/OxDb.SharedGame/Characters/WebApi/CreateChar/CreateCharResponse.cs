using MessagePack;
using OxDb.SharedCore.Website.Responses.Interfaces;
using OxDb.SharedGame.Characters.PlayerData;
using System.Collections.Generic;

namespace OxDb.SharedGame.Characters.WebApi.CreateChar
{
    public class CreateCharResponse : IWebResponse
    {
        [IgnoreMember] public Character NewChar { get; set; }
        public List<CharacterStub> AllCharacters { get; set; }
    }
}


