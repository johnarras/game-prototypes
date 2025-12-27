using MessagePack;
using Genrpg.Shared.DataStores.Categories.PlayerData.ParentChild;
using Genrpg.Shared.Units.Loaders;
using Genrpg.Shared.Units.Mappers;
using System.Collections.Generic;

namespace Genrpg.Shared.Achievements.PlayerData
{
    /// <summary>
    /// Used to contain a list of currencies on objects that need it (like user and character)
    /// </summary>

    [MessagePackObject]
    public class AchievementData : OwnerQuantityObjectList<AchievementStatus>
    {
        [Key(0)] public override string Id { get; set; }

        public long GetQuantity(long AchievementTypeId)
        {
            return Get(AchievementTypeId).Quantity;
        }

    }
    [MessagePackObject]
    public class AchievementStatus : OwnerQuantityChild
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string OwnerId { get; set; }
        [Key(2)] public override long IdKey { get; set; }
        [Key(3)] public override long Quantity { get; set; }

    }

    [MessagePackObject]
    public class AchievementDto : OwnerDtoList<AchievementData, AchievementStatus>
    {
        [Key(0)] public override List<AchievementStatus> Children { get; set; }
        [Key(1)] public override AchievementData Parent { get; set; }
        [Key(2)] public override string Id { get; set; }
    }
    public class AchievementDataLoader : OwnerIdDataLoader<AchievementData, AchievementStatus> { }


    public class AchievementDataMapper : OwnerDataMapper<AchievementData, AchievementStatus, AchievementDto> { }
}


