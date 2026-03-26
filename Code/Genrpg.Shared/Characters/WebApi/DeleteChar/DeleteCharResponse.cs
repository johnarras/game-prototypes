using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Website.Interfaces;
using System.Collections.Generic;

namespace Genrpg.Shared.Characters.WebApi.DeleteChar
{
    public class DeleteCharResponse : IWebResponse
    {
        public List<CharacterStub> AllCharacters { get; set; }
    }
}


