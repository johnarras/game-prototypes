using MessagePack;
using OxDb.SharedCore.Utils.Data;
using OxDb.SharedGame.Core.PlayerData;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Constants;
using OxDb.SharedGame.DataStores.Categories.PlayerData.NoChild;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Users;
using OxDb.SharedGame.Units.Loaders;
using OxDb.SharedGame.Units.Mappers;

namespace OxDb.SharedGame.Trader.Holdings.PlayerData
{

    /// <summary>
    /// This is modified only when the player buys or sells items or adds or removes CaravanMembers.
    /// </summary>
    [MessagePackObject]
    public class HoldingsData : UniquePersonalUserData, IUserData
    {
        public override int GetOffsetBit() { return EPersonalDataOffsetBits.Holdings; }


        public override PersonalDataAccumulation GetAccumulation()
        {
            PersonalDataAccumulation accumulation = new PersonalDataAccumulation()
            {
            };

            accumulation.SumValues.Add(CaravanMembersOwned.GetBitCount());
            accumulation.SumValues.Add(SkinsOwned.GetBitCount());
            accumulation.SumValues.Add(CitiesVisited.GetBitCount());

            return accumulation;
        }


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


