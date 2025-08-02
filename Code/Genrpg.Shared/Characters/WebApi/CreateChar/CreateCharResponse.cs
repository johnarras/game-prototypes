using MessagePack;
using System.Collections.Generic;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Website.Interfaces;

namespace Genrpg.Shared.Characters.WebApi.CreateChar
{
    [MessagePackObject]
    public class CreateCharResponse : IWebResponse
    {
        [IgnoreMember] public Character NewChar { get; set; }
        [Key(0)] public List<CharacterStub> AllCharacters { get; set; }
    }
}
