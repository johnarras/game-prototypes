namespace Genrpg.Shared.Core.Constants
{

    public enum EGameModes
    {
        Crawler = 0,
        MMO = 1,
        BoardGame = 2,
        Trader = 3,
    }

    public class GameModeUtils
    {
        public static bool IsPureClientMode(EGameModes mode)
        {
            return mode == EGameModes.Crawler;
        }

        public static bool IsCrawlerMode(EGameModes mode)
        {
            return mode == EGameModes.Crawler;
        }
    }

}
