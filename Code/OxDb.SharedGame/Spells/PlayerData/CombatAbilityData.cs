using MessagePack;
using OxDb.SharedGame.DataStores.Categories.PlayerData.ParentChild;
using OxDb.SharedGame.Units.Loaders;
using OxDb.SharedGame.Units.Mappers;
using System.Collections.Generic;

namespace OxDb.SharedGame.Spells.PlayerData
{
    [MessagePackObject]
    public class CombatAbilityRank : OwnerPlayerData
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string OwnerId { get; set; }
        [Key(2)] public long AbilityCategoryId { get; set; }
        [Key(3)] public long AbilityTypeId { get; set; }
        [Key(4)] public int Rank { get; set; }
    }
    [MessagePackObject]
    public class CombatAbilityData : OwnerObjectList<CombatAbilityRank>
    {
        [Key(0)] public override string Id { get; set; }

    }

    public class CombatAbilityDataLoader : OwnerDataLoader<CombatAbilityData, CombatAbilityRank> { }

    [MessagePackObject]
    public class CombatAbilityDto : OwnerDtoList<CombatAbilityData, CombatAbilityRank>
    {
        [Key(0)] public override List<CombatAbilityRank> Children { get; set; }
        [Key(1)] public override CombatAbilityData Parent { get; set; }
        [Key(2)] public override string Id { get; set; }
    }

    public class CombatAbilityDataMapper : OwnerDataMapper<CombatAbilityData, CombatAbilityRank, CombatAbilityDto> { }

}


