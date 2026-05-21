namespace OxDb.SharedCore.Utils
{
    public static class MathExtensionMethods
    {

        public static int SafeMod(this int val, int mod)
        {
            while (val < 0)
            {
                val += mod;
            }
            return val % mod;
        }

        public static long SafeMod(this long val, long mod)
        {
            while (val < 0)
            {
                val += mod;
            }
            return val % mod;
        }
    }
}
