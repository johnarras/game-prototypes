namespace OxDb.SharedCore.Core.Constants
{

    public enum EGameModes
    {
        Crawler = 0,
        MMO = 1,
        Minigames = 2,
        Trader = 3,
        LockstepTemplate = 4,
    }

    public class GameModeUtils
    {
        public static bool IsPureClientMode(EGameModes mode)
        {
            return mode == EGameModes.Crawler || mode == EGameModes.LockstepTemplate;
        }


        public static bool IsMultiCharacterMode(string modeName)
        {
            return modeName == EGameModes.MMO.ToString();
        }

        public static bool IsMultiCharacterMode(EGameModes mode)
        {
            return mode == EGameModes.MMO;
        }
    }
}


