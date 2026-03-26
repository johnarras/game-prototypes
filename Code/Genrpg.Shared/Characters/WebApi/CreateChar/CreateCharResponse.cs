using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Website.Interfaces;
using MessagePack;
using System.Collections.Generic;

namespace Genrpg.Shared.Characters.WebApi.CreateChar
{
    public class CreateCharResponse : IWebResponse
    {
        [IgnoreMember] public Character NewChar { get; set; }
        public List<CharacterStub> AllCharacters { get; set; }
    }
}


