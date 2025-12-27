using MessagePack;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Website.Interfaces;

namespace Genrpg.Shared.Characters.WebApi.DeleteChar
{
    public class DeleteCharResponse : IWebResponse
    {
        public List<CharacterStub> AllCharacters { get; set; }
    }
}


