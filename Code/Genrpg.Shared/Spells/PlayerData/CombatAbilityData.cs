using MessagePack;

using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Units.Loaders;
using Genrpg.Shared.Units.Mappers;
using Genrpg.Shared.DataStores.Categories.PlayerData.ParentChild;

namespace Genrpg.Shared.Spells.PlayerData
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

    public class CombatAbilityDto : OwnerDtoList<CombatAbilityData, CombatAbilityRank> { }

    public class CombatAbilityDataMapper : OwnerDataMapper<CombatAbilityData, CombatAbilityRank, CombatAbilityDto> { }

}
