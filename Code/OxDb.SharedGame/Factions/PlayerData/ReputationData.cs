using MessagePack;
using OxDb.SharedCore.Utils.Data;
using OxDb.SharedGame.DataStores.Categories.PlayerData.NoChild;
using OxDb.SharedGame.Units.Mappers;
namespace OxDb.SharedGame.Factions.PlayerData
{
    /// <summary>
    /// Used to contain a list of currencies on objects that need it (like user and character)
    /// </summary>

    [MessagePackObject]
    public class ReputationData : NoChildPlayerData
    {
        [Key(0)] public override string Id { get; set; }

        [Key(1)] public SmallIdLongCollection Data { get; set; } = new SmallIdLongCollection();

    }

    [MessagePackObject]
    public class ReputationDto : NoChildPlayerDataDto<ReputationData>
    {
        [Key(0)] public override ReputationData Parent { get; set; }
        [Key(1)] public override string Id { get; set; }
    }


    public class ReputationDataMapper : NoChildUnitDataMapper<ReputationData, ReputationDto> { }
}


