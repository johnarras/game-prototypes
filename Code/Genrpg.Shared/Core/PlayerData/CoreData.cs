using Genrpg.Shared.Core.Constants;
using Genrpg.Shared.DataStores.Categories.PlayerData.NoChild;
using Genrpg.Shared.DataStores.Categories.PlayerData.Users;
using Genrpg.Shared.DataStores.Constants;
using Genrpg.Shared.GameSettings.PlayerData;
using Genrpg.Shared.PlayerFiltering.Interfaces;
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
    public class CoreData : UniquePersonalUserData, IUserData, IFilteredObject
    {
        [Key(0)] public override string Id { get; set; }

        public string GetId() { return pk; }

        [Key(1)] public DateTime CreationDate { get; set; }
        [Key(2)] public string ClientVersion { get; set; } = VersionConstants.MinVersion.ToString();
        [Key(3)] public GameDataOverrideList DataOverrides { get; set; } = new GameDataOverrideList();
        [Key(4)] public DateTime LastDailyReset { get; set; }
        [Key(5)] public DateTime NextHourlyUpdate { get; set; }

        [Key(6)] public long Level { get; set; }

        [Key(7)] public SmallIdLongCollection Currencies { get; set; } = new SmallIdLongCollection();

        [Key(8)] public SmallIdLongCollection Vars { get; set; } = new SmallIdLongCollection();

        public bool HasFlag(long flagIndex) { return (Vars[CoreVars.Flags] & (1 << (int)flagIndex)) != 0; }
        public void AddFlag(long flagIndex) { Vars[CoreVars.Flags] = Vars[CoreVars.Flags] | (long)(1 << (int)flagIndex); }
        public void RemoveFlag(long flagIndex) { Vars[CoreVars.Flags] = Vars[CoreVars.Flags] & ~(1 << (int)flagIndex); }

        public void SetNextHourlyUpdate()
        {
            DateTime nowTime = DateTime.UtcNow;
            NextHourlyUpdate = nowTime.Date.AddHours(nowTime.Hour + 1);
        }
    }

    public class CoreDataLoader : UnitDataLoader<CoreData> { }


    [MessagePackObject]
    public class CoreDataDto : NoChildPlayerDataDto<CoreData>
    {
        [Key(0)] public override CoreData Parent { get; set; }
        [Key(1)] public override string Id { get; set; }
    }


    public class CoreDataMapper : NoChildUnitDataMapper<CoreData, CoreDataDto> { }
}


