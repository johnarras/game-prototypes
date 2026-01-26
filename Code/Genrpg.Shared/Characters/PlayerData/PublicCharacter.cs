using Genrpg.Shared.DataStores.Categories.ContentData;

namespace Genrpg.Shared.Characters.PlayerData
{
    public class PublicCharacter : BasePublicPlayerData
    {
        public override string Id { get; set; }
        public string Name { get; set; }
        public long FactionTypeId { get; set; }
        public long UnitTypeId { get; set; }
        public long SexTypeId { get; set; }

    }
}


