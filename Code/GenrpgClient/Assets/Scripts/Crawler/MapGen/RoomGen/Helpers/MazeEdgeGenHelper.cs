using Assets.Scripts.Crawler.MapGen.RoomGen.Entities;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Crawler.MapGen.Constants;

namespace Assets.Scripts.Crawler.MapGen.RoomGen.Helpers
{
    public class MazeEdgeGenHelper : BaseEdgeGenHelper
    {
        public override long HelperKey => RoomEdgeTypes.Maze;

        protected override int GetRowLength(RoomEdgeGenArgs edgeArgs, int pindex, IRandom rand)
        {
            return edgeArgs.GetMaxLength();
        }
    }
}
