namespace OxDb.SharedGame.Crawler.Maps.Constants
{
    public class WallTypes
    {
        public const int None = 0;
        public const int Wall = 1;
        public const int Door = 2;
        public const int SecretFromSW = 3;
        public const int SecretFromNE = 4;
        public const int SecretBoth = 5;
        public const int Barricade = 6;
        public const int Max = 7;

        public const int Building = 12;
        public static bool IsBlockingTypeFromDir(long wallType, int dx, int dz)
        {
            if (wallType == Wall || wallType == Barricade)
            {
                return true;
            }

            if ((dx > 0 || dz > 0) && wallType == SecretFromSW)
            {
                return true;
            }

            if ((dx < 0 || dz < 0) && wallType == SecretFromNE)
            {
                return true;
            }

            return false;
               
        }

        public static bool UsesTilemapWallArt(long wallType)
        {
            return wallType == Wall || wallType == SecretFromSW || wallType == SecretBoth || wallType == SecretFromNE;
        }

        public static bool HasPillar(long wallType)
        {
            return wallType != None && wallType != Barricade;
        }
    }
}


