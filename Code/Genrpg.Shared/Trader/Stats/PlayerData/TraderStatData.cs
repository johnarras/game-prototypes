using Genrpg.Shared.DataStores.Categories.PlayerData.NoChild;
using Genrpg.Shared.DataStores.Categories.PlayerData.Users;
using Genrpg.Shared.Units.Loaders;
using Genrpg.Shared.Units.Mappers;
using Genrpg.Shared.Utils.Data;
using MessagePack;

namespace Genrpg.Shared.Currencies.PlayerData
{
    /// <summary>
    /// Used to contain a list of currencies on objects that need it (like user and character)
    /// </summary>

    [MessagePackObject]
    public class TraderStatData : NoChildPlayerData, IUserData
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public TraderStatCollection Stats { get; set; } = new TraderStatCollection();
    }

    [MessagePackObject]
    public class TraderStatCollection : BaseSmallIdObjectCollection<TraderStatStatus>
    {
        [Key(0)] public override TraderStatStatus[] Data { get; set; } = new TraderStatStatus[4];
    }

    [MessagePackObject]
    public class TraderStatStatus
    {
        [Key(0)] public long Base { get; set; }
        [Key(1)] public long Bonus { get; set; }

        public long Max() { return Base + Bonus; }

        public void RaiseBaseToValue(long newBase)
        {
            if (Base < newBase)
            {
                Base = newBase;
            }
        }

        public void AddBonusValue(long bonus)
        {
            if (bonus > 0)
            {
                Bonus += bonus;
            }
        }
    }

    public class TraderStatDataLoader : UnitDataLoader<TraderStatData> { }


    public class TraderStatDto : NoChildPlayerDataDto<TraderStatData> { }


    public class TraderStatDataMapper : NoChildUnitDataMapper<TraderStatData, TraderStatDto> { }
}
