using MessagePack;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Units.Loaders;
using Genrpg.Shared.Units.Mappers;
using Genrpg.Shared.DataStores.Categories.PlayerData.ParentChild;

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

    public class AchievementDto : OwnerDtoList<AchievementData, AchievementStatus> { }
    public class AchievementDataLoader : OwnerIdDataLoader<AchievementData, AchievementStatus> { }


    public class AchievementDataMapper : OwnerDataMapper<AchievementData, AchievementStatus, AchievementDto> { }
}
