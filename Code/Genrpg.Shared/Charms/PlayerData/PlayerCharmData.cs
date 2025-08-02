using Genrpg.Shared.DataStores.Categories.PlayerData.ParentChild;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Units.Loaders;
using Genrpg.Shared.Units.Mappers;
using MessagePack;
using System.Collections.Generic;

namespace Genrpg.Shared.Charms.PlayerData
{
    [MessagePackObject]
    public class PlayerCharmBonus
    {
        [Key(0)] public long EntityTypeId { get; set; }
        [Key(1)] public long EntityId { get; set; }
        [Key(2)] public long Quantity { get; set; }
    }
    [MessagePackObject]
    public class PlayerCharm : OwnerPlayerData, IId
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string OwnerId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public string Hash { get; set; }
        [Key(4)] public long CurrentCharmUseId { get; set; }
        [Key(5)] public string TargetId { get; set; }
        [Key(6)] public string TargetName { get; set; }

        [Key(7)] public List<PlayerCharmBonusList> Bonuses { get; set; } = new List<PlayerCharmBonusList>();

    }
    [MessagePackObject]
    public class PlayerCharmBonusList
    {
        [Key(0)] public long CharmUseId { get; set; }

        [Key(1)] public List<PlayerCharmBonus> Bonuses { get; set; } = new List<PlayerCharmBonus>();

    }
    [MessagePackObject]
    public class PlayerCharmData : OwnerIdObjectList<PlayerCharm>
    {
        [Key(0)] public override string Id { get; set; }
    }
    public class PlayerCharmDto : OwnerDtoList<PlayerCharmData, PlayerCharm> { }


    public class CharmDataLoader : OwnerIdDataLoader<PlayerCharmData, PlayerCharm>
    {
    }

    public class CharmDataMapper : OwnerIdDataMapper<PlayerCharmData, PlayerCharm, PlayerCharmDto> { }
}
