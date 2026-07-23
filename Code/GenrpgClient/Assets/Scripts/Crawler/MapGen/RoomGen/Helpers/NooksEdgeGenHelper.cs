using OxDb.Client.Crawler.MapGen.RoomGen.Entities;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Crawler.MapGen.Constants;

namespace OxDb.Client.Crawler.MapGen.RoomGen.Helpers
{
    public class NooksEdgeGenHelper : BaseEdgeGenHelper
    {
        public override long HelperKey => RoomEdgeTypes.Nooks;

        protected override int GetRowLength(RoomEdgeGenArgs edgeArgs, int pindex, IRandom rand)
        {
            return 1;
        }
    }
}
