using Genrpg.Shared.DataStores.Categories.PlayerData.NoChild;
using Genrpg.Shared.DataStores.Categories.PlayerData.Users;
using Genrpg.Shared.DataStores.Constants;
using Genrpg.Shared.GameSettings.PlayerData;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using Genrpg.Shared.Trader.Caravans.Entities;
using Genrpg.Shared.Units.Loaders;
using Genrpg.Shared.Units.Mappers;
using Genrpg.Shared.Utils.Data;
using MessagePack;
using System;

namespace Genrpg.Shared.Core.PlayerData
{




    /// <summary>
    /// Core data about the board user
    /// </summary>
    [MessagePackObject]
    public class CoreUserData : UniquePersonalUserData, IUserData, IFilteredObject
    {
        [Key(0)] public override string Id { get; set; }

        [Key(1)] public DateTime CreationDate { get; set; }
        [Key(2)] public string ClientVersion { get; set; } = VersionConstants.MinVersion.ToString();
        [Key(3)] public GameDataOverrideList DataOverrides { get; set; } = new GameDataOverrideList();
        [Key(4)] public DateTime LastDailyReset { get; set; }
        [Key(5)] public DateTime NextHourlyUpdate { get; set; }

        [Key(6)] public bool FastMove { get; set; }

        [Key(7)] public SmallIdLongCollection Currencies { get; set; } = new SmallIdLongCollection();

        [Key(8)] public long Plays { get; set; }
        [Key(9)] public long Level { get; set; }
        [Key(10)] public long Exp { get; set; }

        [Key(11)] public long Mult { get; set; }
        [Key(12)] public long Dice { get; set; }
        [Key(13)] public long Bonus { get; set; }
        [Key(14)] public long Cost { get; set; }
        [Key(15)] public long Foraging { get; set; }
        [Key(16)] public long Guards { get; set; }
        [Key(17)] public long OverloadCount { get; set; }


        /// <summary>
        /// If this is 0, we are in CityId, otherwise we are on the road toward CityId.
        /// </summary>
        [Key(18)] public long RoadId { get; set; }
        /// <summary>
        /// This should always be nonzero, if RoadId is nonzero, this is the target city
        /// </summary>
        [Key(19)] public long CityId { get; set; }
        /// <summary>
        /// Distance gone along the road.
        /// </summary>
        [Key(20)] public long Dist { get; set; }

        [Key(21)] public long Day { get; set; }

        public CaravanPosition GetPosition()
        {
            CaravanPosition pos = new CaravanPosition()
            {
                RoadId = RoadId,
                DistanceTravelled = Dist,
            };

            if (RoadId > 0)
            {
                pos.CityId = 0;
                pos.TargetCityId = CityId;
            }
            else
            {
                pos.CityId = CityId;
                pos.RoadId = 0;
                pos.DistanceTravelled = 0;
            }

            return pos;
        }

        public void SetNextHourlyUpdate()
        {
            DateTime nowTime = DateTime.UtcNow;
            NextHourlyUpdate = nowTime.Date.AddHours(nowTime.Hour + 1);
        }
    }

    public class CoreUserDataLoader : UnitDataLoader<CoreUserData> { }


    [MessagePackObject]
    public class CoreUserDto : NoChildPlayerDataDto<CoreUserData>
    {
        [Key(0)] public override CoreUserData Parent { get; set; }
        [Key(1)] public override string Id { get; set; }
    }


    public class CoreUserDataMapper : NoChildUnitDataMapper<CoreUserData, CoreUserDto> { }
}


