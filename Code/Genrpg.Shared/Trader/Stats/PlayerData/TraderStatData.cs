using Genrpg.Shared.DataStores.Categories.PlayerData.NoChild;
using Genrpg.Shared.DataStores.Categories.PlayerData.Users;
using Genrpg.Shared.Units.Loaders;
using Genrpg.Shared.Units.Mappers;
using Genrpg.Shared.Utils.Data;
using MessagePack;
using System;

namespace Genrpg.Shared.Trader.Stats.PlayerData
{
    /// <summary>
    /// Used to contain a list of currencies on objects that need it (like user and character)
    /// </summary>

    [MessagePackObject]
    public class TraderStatData : UniquePersonalUserData, IUserData
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public TraderStatCollection Stats { get; set; } = new TraderStatCollection();
    }

    [MessagePackObject]
    public class TraderStatCollection : BaseSmallIdObjectCollection<TraderStatStatus>
    {
        [Key(0)] public TraderStatStatus[] Data { get => _data; set => _data = value; }
        protected override TraderStatStatus InternalAdd(TraderStatStatus first, TraderStatStatus second)
        {
            throw new NotImplementedException("Cannot add two TraderStatStatuses together");
        }

        protected override bool IsDefault(TraderStatStatus t)
        {
            return t == default(TraderStatStatus);
        }
    }

    [MessagePackObject]
    public class TraderStatStatus
    {
        [Key(0)] public int Base { get; set; }
        [Key(1)] public int Bonus { get; set; }

        public int Total() { return Base + Bonus; }

    }

    public class TraderStatDataLoader : UnitDataLoader<TraderStatData> { }


    [MessagePackObject]
    public class TraderStatDto : NoChildPlayerDataDto<TraderStatData>
    {
        [Key(0)] public override TraderStatData Parent { get; set; }
        [Key(1)] public override string Id { get; set; }
    }


    public class TraderStatDataMapper : NoChildUnitDataMapper<TraderStatData, TraderStatDto> { }
}


