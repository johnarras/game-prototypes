using OxDb.Client.Crawler.Maps.GameObjects;
using OxDb.Client.Crawler.Services.CrawlerMaps;
using OxDb.SharedGame.Crawler.Worlds.Entities;

namespace OxDb.Client.Crawler.Maps.Services.Entities
{
    public class CrawlerMoveStatus
    {
        public MovementKeyCode KeyCode;
        public bool MoveIsStopped;
        public bool MovedPosition;
        public bool IsRotation;
        public CrawlerWorld World;
        public CrawlerMapRoot MapRoot;
        public int BlockBits;
        public int SX;
        public int SZ;
        public int EX;
        public int EZ;

    }
}


