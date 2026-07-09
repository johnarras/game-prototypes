using Assets.Scripts.Crawler.MapGen.Helpers;
using Assets.Scripts.Crawler.MapGen.RoomGen.Entities;
using OxDb.SharedGame.Crawler.MapGen.Constants;
using OxDb.SharedGame.Crawler.Maps.Entities;
using System.Collections.Generic;

namespace Assets.Scripts.Crawler.MapGen.RoomGen.Helpers
{
    public class FlatEdgeGenHelper : BaseEdgeGenHelper
    {
        public override long HelperKey => RoomEdgeTypes.Flat;

        protected override List<EdgeRowArgs> GetEdgeRowArgs(RoomEdgeGenArgs edgeArgs, CrawlerMapGenData genData, DungeonLevelGenArgs levelArgs)
        {
            return new List<EdgeRowArgs>();
        }
    }
}
