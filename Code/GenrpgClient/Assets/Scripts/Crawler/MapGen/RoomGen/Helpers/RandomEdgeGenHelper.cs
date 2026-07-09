using Assets.Scripts.Crawler.MapGen.RoomGen.Entities;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Crawler.MapGen.Constants;

namespace Assets.Scripts.Crawler.MapGen.RoomGen.Helpers
{
    public class RandomEdgeGenHelper : BaseEdgeGenHelper
    {
        public override long HelperKey => RoomEdgeTypes.Random;

        protected override int GetRowLength(RoomEdgeGenArgs edgeArgs, int pindex, IRandom rand)
        {
            return RandUtils.IntRange(1, edgeArgs.GetMaxLength(), rand);
        }
    }
}
