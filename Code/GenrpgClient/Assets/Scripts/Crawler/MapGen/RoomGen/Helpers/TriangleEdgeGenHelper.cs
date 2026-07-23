using OxDb.Client.Crawler.MapGen.RoomGen.Entities;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Crawler.MapGen.Constants;
using System;

namespace OxDb.Client.Crawler.MapGen.RoomGen.Helpers
{
    public class TriangleEdgeGenHelper : BaseEdgeGenHelper
    {
        public override long HelperKey => RoomEdgeTypes.Triangle;

        protected override int GetRowLength(RoomEdgeGenArgs edgeArgs, int pindex, IRandom rand)
        {
            EdgePositionPercents percents = edgeArgs.GetPositionPercents(pindex);


            float percent = percents.PercentFromMid;

            if (edgeArgs.LeftOffset)
            {
                percent = (1 - percents.PercentFromLeft);
            }
            else if (edgeArgs.RightOffset)
            {
                percent = 1 - percents.PercentFromRight;
            }
            else
            {
                percent = 1 - percents.PercentFromMid;
            }

            return 1 + (int)Math.Round(percent * edgeArgs.GetMaxLength());
        }
    }
}
