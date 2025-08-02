using Assets.Scripts.Crawler.Maps.GameObjects;
using Assets.Scripts.Crawler.Services.CrawlerMaps;
using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Worlds.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Crawler.Maps.Services.Entities
{
    public class CrawlerMoveStatus
    {
        public MovementKeyCode KeyCode;
        public bool MoveIsComplete;
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
