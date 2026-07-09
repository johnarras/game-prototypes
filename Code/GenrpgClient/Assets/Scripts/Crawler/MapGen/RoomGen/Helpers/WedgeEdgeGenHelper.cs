using Assets.Scripts.Crawler.MapGen.RoomGen.Entities;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Crawler.MapGen.Constants;

namespace Assets.Scripts.Crawler.MapGen.RoomGen.Helpers
{
    public class WedgeEdgeGenHelper : BaseEdgeGenHelper
    {
        public override long HelperKey => RoomEdgeTypes.Wedge;

        protected override int GetRowLength(RoomEdgeGenArgs edgeArgs, int pindex, IRandom rand)
        {
            EdgePositionPercents percents = edgeArgs.GetPositionPercents(pindex);

            return 1 + (int)(percents.PercentFromMid * edgeArgs.GetMaxLength());
        }
    }
}
