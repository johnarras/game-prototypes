using Genrpg.Shared.DataStores.Categories.PlayerData.NoChild;
using Genrpg.Shared.Units.Mappers;
using Genrpg.Shared.Utils.Data;
using MessagePack;
namespace Genrpg.Shared.Achievements.PlayerData
{
    /// <summary>
    /// Used to contain a list of currencies on objects that need it (like user and character)
    /// </summary>

    [MessagePackObject]
    public class AchievementData : NoChildPlayerData
    {
        [Key(0)] public override string Id { get; set; }

        [Key(1)] public SmallIdLongCollection Data { get; set; } = new SmallIdLongCollection();

    }

    [MessagePackObject]
    public class AchievementDto : NoChildPlayerDataDto<AchievementData>
    {
        [Key(0)] public override AchievementData Parent { get; set; }
        [Key(1)] public override string Id { get; set; }
    }


    public class AchievementDataMapper : NoChildUnitDataMapper<AchievementData, AchievementDto> { }
}


