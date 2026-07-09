using MessagePack;
using OxDb.SharedCore.Utils.Data;
using OxDb.SharedGame.Currencies.PlayerData;
using OxDb.SharedGame.DataStores.Categories.PlayerData.NoChild;
using OxDb.SharedGame.Units.Loaders;
using OxDb.SharedGame.Units.Mappers;
namespace OxDb.SharedGame.Achievements.PlayerData
{
    /// <summary>
    /// Used to contain a list of currencies on objects that need it (like user and character)
    /// </summary>

    [MessagePackObject]
    public class AchievementData : NoChildIndexedUserData
    {
        [Key(0)] public override string Id { get; set; }

        [Key(1)] public SmallIdLongCollection Data { get; set; } = new SmallIdLongCollection();
        [Key(2)] public override string VersionTag { get; set; }

    }

    public class AchievementLoader : UnitDataLoader<AchievementData> { }
    [MessagePackObject]
    public class AchievementDto : NoChildPlayerDataDto<AchievementData>
    {
        [Key(0)] public override AchievementData Parent { get; set; }
        [Key(1)] public override string Id { get; set; }
    }


    public class AchievementDataMapper : NoChildUnitDataMapper<AchievementData, AchievementDto> { }
}


