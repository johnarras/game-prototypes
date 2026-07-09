using OxDb.SharedCore.Core.Constants;

namespace OxDb.SharedGame.Trader.Constants
{
    public class TraderVars
    {
        public const long Flags = CoreVars.Flags;
        public const long Mult = CoreVars.Mult;
        public const long PlayCount = CoreVars.PlayCount;

        public const long MaxInventory = 4; // Calc
        public const long InventoryUsed = 5;
        public const long BaseDiceSpeed = 6; // May be 0 if you can't move.
        public const long BonusSpeedPerDie = 7; // Calc
        public const long FromX = 8;
        public const long FromZ = 9;
        public const long ToX = 10;
        public const long ToZ = 11;
        public const long CityId = 12;
        public const long TotalDistanceToTarget = 13;
        public const long DistanceGone = 14;
        public const long Searching = 15; // Calc
        public const long Luck = 16; // Calc
        public const long ExpToLevelUp = 17;
        public const long NextDebuffEndsDay = 18;
        public const long DebuffBits = 19;
        public const long DebuffDaysPlayed = 20;
        public const long BuffBits = 21;
        public const long MaxSize = 22; // Calc
        public const long SizeUsed = 23;
        public const long MultBonusSpeed = 24;
    }
}
