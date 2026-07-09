using Assets.Scripts.Crawler.MapGen.Helpers;
using Assets.Scripts.Crawler.MapGen.RoomGen.Services;
using Assets.Scripts.Crawler.MapGen.Services;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedGame.Crawler.Maps.Entities;
using OxDb.SharedGame.ProcGen.Services;
using System.Threading.Tasks;

namespace Assets.Scripts.Crawler.MapGen.DungeonGen.Helpers
{
    public abstract class BaseDungeonGenHelper : IDungeonGenHelper
    {
        protected ISamplingService _samplingService = null;
        protected ILineGenService _lineGenService = null;
        protected ILogService _logService = null;
        protected ICrawlerMapGenService _mapGenService = null;
        protected IGameData _gameData = null;
        protected IClientGameState _gs = null;
        protected IRoomGenService _roomGenService = null;

        public abstract long HelperKey { get; }

        public abstract ValueTask<bool> GenerateLevel(CrawlerMapGenData genData, DungeonLevelGenArgs levelArgs);


        protected void AddWalls(CrawlerMapGenData genData, DungeonLevelGenArgs levelArgs)
        {
            CrawlerMap map = levelArgs.Map;
            for (int x = 0; x < map.Width; x++)
            {
                for (int z = 0; z < map.Height; z++)
                {

                }
            }
        }

    }
}
