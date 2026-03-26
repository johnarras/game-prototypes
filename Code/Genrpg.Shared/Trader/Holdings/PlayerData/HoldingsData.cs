using Genrpg.Shared.DataStores.Categories.PlayerData.NoChild;
using Genrpg.Shared.DataStores.Categories.PlayerData.Users;
using Genrpg.Shared.Units.Loaders;
using Genrpg.Shared.Units.Mappers;
using Genrpg.Shared.Utils.Data;
using MessagePack;

namespace Genrpg.Shared.Trader.Holdings.PlayerData
{

    /// <summary>
    /// This is modified only when the player buys or sells items or adds or removes CaravanMembers.
    /// </summary>
    [MessagePackObject]
    public class HoldingsData : UniquePersonalUserData, IUserData
    {
        [Key(0)] public override string Id { get; set; }

        [Key(1)] public SmallIndexBitList CaravanMembersOwned { get; set; } = new SmallIndexBitList();

        [Key(2)] public SmallIndexBitList SkinsOwned { get; set; } = new SmallIndexBitList();

        [Key(3)] public SmallIndexBitList CitiesVisited { get; set; } = new SmallIndexBitList();
    }



    public class HoldingsDataLoader : UnitDataLoader<HoldingsData> { }


    [MessagePackObject]
    public class HoldingsDto : NoChildPlayerDataDto<HoldingsData>
    {
        [Key(0)] public override HoldingsData Parent { get; set; }
        [Key(1)] public override string Id { get; set; }
    }


    public class HoldingsDataMapper : NoChildUnitDataMapper<HoldingsData, HoldingsDto> { }
}


