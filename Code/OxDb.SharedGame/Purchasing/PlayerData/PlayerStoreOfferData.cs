using MessagePack;
using OxDb.SharedCore.DataStores.Interfaces;
using OxDb.SharedCore.Rewards.Entities;
using OxDb.SharedGame.Core.PlayerData;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Constants;
using OxDb.SharedGame.DataStores.Categories.PlayerData.NoChild;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Users;
using OxDb.SharedGame.Units.Loaders;
using OxDb.SharedGame.Units.Mappers;
using System;
using System.Collections.Generic;

namespace OxDb.SharedGame.Purchasing.PlayerData
{
    [MessagePackObject]
    public class PlayerStoreOffer
    {
        [Key(0)] public long IdKey { get; set; }
        [Key(1)] public string OfferId { get; set; }
        [Key(2)] public string Name { get; set; }
        [Key(3)] public string Desc { get; set; }
        [Key(4)] public string Art { get; set; }
        [Key(5)] public string Icon { get; set; }
        [Key(6)] public long StoreFeatureId { get; set; }
        [Key(7)] public long StoreSlotId { get; set; }
        [Key(8)] public long StoreThemeId { get; set; }
        [Key(9)] public DateTime EndDate { get; set; }
        [Key(10)] public List<PlayerBundle> Bundles { get; set; } = new List<PlayerBundle>();
    }

    [MessagePackObject]
    public class PlayerBundle
    {
        [Key(0)] public long Index { get; set; }
        [Key(1)] public long ProductSkuId { get; set; }
        [Key(2)] public string UniqueId { get; set; }
        [Key(3)] public string BundleId { get; set; }
        [Key(4)] public List<Reward> Rewards { get; set; } = new List<Reward>();
    }


    [MessagePackObject]
    public class PlayerStoreOfferData : UniquePersonalUserData, IUserData, IServerOnlyData
    {
        public override int GetOffsetBit() { return PersonalDataOffsetBits.StoreOffers; }
        public override PersonalDataAccumulation GetAccumulation()
        {
            return new PersonalDataAccumulation();
        }

        [Key(0)] public override string Id { get; set; }

        [Key(1)] public DateTime GameDataSaveTime { get; set; } = DateTime.UtcNow;

        [Key(2)] public DateTime LastTimeSet { get; set; } = DateTime.MinValue;

        [Key(3)] public List<PlayerStoreOffer> StoreOffers { get; set; } = new List<PlayerStoreOffer>();
    }

    [MessagePackObject]
    public class PlayerStoreOfferDto : NoChildPlayerDataDto<PlayerStoreOfferData>
    {
        [Key(0)] public override PlayerStoreOfferData Parent { get; set; }
        [Key(1)] public override string Id { get; set; }
    }


    public class PlayerStoreOfferDataMapper : NoChildUnitDataMapper<PlayerStoreOfferData, PlayerStoreOfferDto> { }


    public class PlayerStoreOfferLoader : UnitDataLoader<PlayerStoreOfferData> { }
}


