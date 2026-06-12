using OxDb.SharedGame.DataStores.Categories.ContentData;

namespace OxDb.SharedGame.Characters.PlayerData
{
    public class PublicCharacter : BasePublicPlayerData
    {
        public override string Id { get; set; }
        public string DisplayName { get; set; }
        public long FactionTypeId { get; set; }
        public long UnitTypeId { get; set; }
        public long SexTypeId { get; set; }

    }
}


