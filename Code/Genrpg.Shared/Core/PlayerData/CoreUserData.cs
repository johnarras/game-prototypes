using Genrpg.Shared.CoreCurrencies.Entities;
using Genrpg.Shared.DataStores.Categories.PlayerData.NoChild;
using Genrpg.Shared.DataStores.Categories.PlayerData.Users;
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
    public class CoreUserData : NoChildPlayerData, IUserData, IFilteredObject
    {
        [Key(0)] public override string Id { get; set; }

        [Key(1)] public DateTime LastDailyReset { get; set; }

        [Key(2)] public CoreCurrencySet Currencies { get; set; } = new CoreCurrencySet();

        [Key(3)] public SmallIdShortCollection Abilities { get; set; } = new SmallIdShortCollection();

        [Key(4)] public SmallIdLongCollection Vars { get; set; } = new SmallIdLongCollection();

        [Key(5)] public DateTime CreationDate { get; set; }

        [Key(6)] public long Level { get; set; }

        [Key(7)] public long Mult { get; set; }

        [Key(8)] public long Plays { get; set; }

        [Key(9)] public string ClientVersion { get; set; }

        [Key(10)] public GameDataOverrideList DataOverrides { get; set; } = new GameDataOverrideList();
    }

    public class CoreUserDataLoader : UnitDataLoader<CoreUserData> { }


    public class CoreUserDto : NoChildPlayerDataDto<CoreUserData> { }


    public class CoreUserDataMapper : NoChildUnitDataMapper<CoreUserData, CoreUserDto> { }
}
