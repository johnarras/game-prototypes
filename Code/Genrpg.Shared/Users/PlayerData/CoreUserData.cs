using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.Currencies.Constants;
using Genrpg.Shared.Units.Loaders;
using Genrpg.Shared.Units.Mappers;
using Genrpg.Shared.Utils.Data;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using Genrpg.Shared.GameSettings.PlayerData;
using Genrpg.Shared.BoardGame.Constants;
using Genrpg.Shared.DataStores.Categories.PlayerData.NoChild;
using Genrpg.Shared.DataStores.Categories.PlayerData.Users;
using Genrpg.Shared.Purchasing.PlayerData;

namespace Genrpg.Shared.Users.PlayerData
{
    /// <summary>
    /// Core data about the board user
    /// </summary>
    [MessagePackObject]
    public class CoreUserData : NoChildPlayerData, IUserData
    {
        [Key(0)] public override string Id { get; set; }

        [Key(1)] public DateTime LastDailyReset { get; set; }

        [Key(2)] public double AvgPlayMult { get; set; } = 1.0f;

        [Key(3)] public SmallIdLongCollection Coins { get; set; } = new SmallIdLongCollection();

        [Key(4)] public SmallIdShortCollection Abilities { get; set; } = new SmallIdShortCollection();

        [Key(5)] public SmallIdLongCollection Vars { get; set; } = new SmallIdLongCollection();

        [Key(6)] public DateTime CreationDate { get; set; }

        [Key(7)] public DateTime LastHourlyReset { get; set; }

        [Key(8)] public List<long> RecentBaseTileTypeIds { get; set; } = new List<long>();
    }

    public class CoreUserDataLoader : UnitDataLoader<CoreUserData> { }


    public class CoreUserDto : NoChildPlayerDataDto<CoreUserData> { }


    public class CoreUserDataMapper : NoChildUnitDataMapper<CoreUserData, CoreUserDto> { }
}
