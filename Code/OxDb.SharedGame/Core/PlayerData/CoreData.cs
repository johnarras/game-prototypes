using MessagePack;
using OxDb.SharedCore.Core.Constants;
using OxDb.SharedCore.DataStores.Constants;
using OxDb.SharedCore.GameSettings.PlayerData;
using OxDb.SharedCore.PlayerFiltering.Interfaces;
using OxDb.SharedCore.Utils.Data;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Constants;
using OxDb.SharedGame.DataStores.Categories.PlayerData.NoChild;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Users;
using OxDb.SharedGame.Trader.Constants;
using OxDb.SharedGame.Units.Loaders;
using OxDb.SharedGame.Units.Mappers;
using System;

namespace OxDb.SharedGame.Core.PlayerData
{




    /// <summary>
    /// Core data about the board user
    /// </summary>
    [MessagePackObject]
    public class CoreData : UniquePersonalUserData, IUserData, IFilteredObject
    {



        public override int GetOffsetBit() { return EPersonalDataOffsetBits.Core; }
        public override PersonalDataAccumulation GetAccumulation()
        {
            return new PersonalDataAccumulation();
        }


        [Key(0)] public override string Id { get; set; }

        public string GetId() { return pk; }

        [Key(1)] public DateTime Created { get; set; } = DateTime.UtcNow;
        [Key(2)] public string Client { get; set; } = VersionConstants.MinVersion.ToString();

        [Key(11)] public string GameSessionId { get; set; }
        [Key(3)] public ABList AB { get; set; } = new ABList();
        [Key(4)] public DateTime NextHourlyUpdate { get; set; }

        [Key(5)] public DateTime NextBuffEndsTime { get; set; }

        [Key(6)] public long UniqueId { get; set; }

        [Key(7)] public long Level { get; set; }

        [Key(8)] public SmallIdLongCollection Currencies { get; set; } = new SmallIdLongCollection();

        [Key(9)] public SmallIdIntCollection Vars { get; set; } = new SmallIdIntCollection();

        [Key(10)] public SmallIdLongCollection TravelDayCurrencies { get; set; } = new SmallIdLongCollection();

        public bool HasFlag(long flagIndex) { return (Vars[CoreVars.Flags] & (1 << (int)flagIndex)) != 0; }
        public void AddFlag(long flagIndex) { Vars[CoreVars.Flags] = Vars[CoreVars.Flags] | (int)(1 << (int)flagIndex); }
        public void RemoveFlag(long flagIndex) { Vars[CoreVars.Flags] = Vars[CoreVars.Flags] & ~(1 << (int)flagIndex); }


        public int GetInventoryOverload()
        {
            return Math.Max(Vars[TraderVars.InventoryUsed] - Vars[TraderVars.MaxInventory], 0);
        }

        public int GetSizeOverload()
        {
            return Math.Max(Vars[TraderVars.SizeUsed] - Vars[TraderVars.MaxSize], 0);
        }


        public int GetDiceSpeed()
        {
            if (GetSizeOverload() > 0)
            {
                return 0;
            }

            return Math.Max(Vars[TraderVars.BaseDiceSpeed] - GetInventoryOverload(), 0);
        }

        public int GetBonusSpeed()
        {

            int diceSpeed = GetDiceSpeed();

            if (diceSpeed == 0)
            {
                return 0;
            }
            return diceSpeed * Vars[TraderVars.BonusSpeedPerDie] + Vars[TraderVars.MultBonusSpeed];
        }


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


