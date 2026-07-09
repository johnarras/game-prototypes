using MessagePack;
using OxDb.SharedCore.Utils.Data;
using OxDb.SharedGame.Core.PlayerData;
using OxDb.SharedGame.Currencies.PlayerData;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Constants;
using OxDb.SharedGame.DataStores.Categories.PlayerData.NoChild;
using OxDb.SharedGame.Units.Loaders;
using OxDb.SharedGame.Units.Mappers;

namespace OxDb.SharedGame.Ads.PlayerData
{

    [MessagePackObject]
    public class AdsData : UniquePersonalUserData
    {
        [Key(0)] public override string Id { get; set; }

        [Key(1)] public int AdsSeenToday { get; set; }

        public override PersonalDataAccumulation GetAccumulation()
        {
            return new PersonalDataAccumulation();
        }

        public override int GetOffsetBit()
        {
            return PersonalDataOffsetBits.Ads;
        }
    }

    public class AdsDataLoader : UnitDataLoader<AdsData> { }


    [MessagePackObject]
    public class AdsDataDto : NoChildPlayerDataDto<AdsData>
    {
        [Key(0)] public override AdsData Parent { get; set; }
        [Key(1)] public override string Id { get; set; }
    }


    public class AdsDataMapper : NoChildUnitDataMapper<AdsData, AdsDataDto> { }
}


