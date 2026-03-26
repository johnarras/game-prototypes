using Genrpg.Shared.Interfaces;

namespace Genrpg.Shared.Characters.PlayerData
{

    public class CharacterStub : IStringId
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public long Level { get; set; }

    }
}


