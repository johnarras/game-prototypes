namespace Genrpg.Shared.Utils
{
    public class FlagUtils
    {
        public static bool MatchesAnyBits(long val, long flag)
        {
            return (val & flag) != 0;
        }

        public static bool HasBitIndex(long val, long bit)
        {
            return MatchesAnyBits(val, (1 << (int)bit));
        }
    }
}


