using OxDb.SharedCore.Interfaces;

namespace OxDb.SharedGame.Characters.PlayerData
{

    public class CharacterStub : IStringId
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public long Level { get; set; }

    }
}


