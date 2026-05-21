using OxDb.SharedCore.Utils;

namespace OxDb.SharedGame.Crawler.Combat.Constants
{
    public class CrawlerCombatConstants
    {
        public const int MinRange = 10;
        public const int MaxRange = 100;
        public const int RangeDelta = 10;

        public const int MaxStartEnemyGroupCount = 6;

        public const long BaseMinDamage = 1;
        public const long BaseMaxDamage = 2;

        public const int PartyCombatGroupIndex = 0;

        public const long SelfSummonPlaceholderId = -1;
        public const long BaseSummonPlaceholderId = -2;

        public const int StartScrollFramesIndex = 4;

        public static readonly int[] ScrollingFramesValues = new int[] { 1, 2, 3, 4, 5, 10, 15 };

        public static int GetScrollingFrames(int scrollFramesIndex)
        {

            scrollFramesIndex = MathUtil.Clamp(0, scrollFramesIndex - 1, ScrollingFramesValues.Length - 1);
            return ScrollingFramesValues[scrollFramesIndex];
        }

        public const int MinGroupCount = 1;
        public const int MinGroupSize = 1;
    }
}


