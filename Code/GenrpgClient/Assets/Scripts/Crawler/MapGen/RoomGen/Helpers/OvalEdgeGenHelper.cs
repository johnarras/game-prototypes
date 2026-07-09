using Assets.Scripts.Crawler.MapGen.RoomGen.Entities;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Crawler.MapGen.Constants;
using System;

namespace Assets.Scripts.Crawler.MapGen.RoomGen.Helpers
{
    public class OvalEdgeGenHelper : BaseEdgeGenHelper
    {
        public override long HelperKey => RoomEdgeTypes.Oval;

        protected override int GetRowLength(RoomEdgeGenArgs edgeArgs, int pindex, IRandom rand)
        {
            EdgePositionPercents percents = edgeArgs.GetPositionPercents(pindex);

            return (int)Math.Ceiling(Math.Sqrt(1 - percents.PercentFromMid) * edgeArgs.GetMaxLength());
        }
    }
}
