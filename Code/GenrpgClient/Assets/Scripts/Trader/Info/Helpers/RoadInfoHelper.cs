using Genrpg.Shared.Crawler.Info.InfoHelpers;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Trader.Roads.Settings;
using System.Collections.Generic;

namespace Assets.Scripts.Trader.Info.Helpers
{
    public class RoadInfoHelper : BaseInfoHelper<RoadSettings, Road>
    {
        public override long HelperKey => EntityTypes.Road;

        public override List<string> GetInfoLines(long entityId)
        {
            List<string> lines = base.GetInfoLines(entityId);


            return lines;

        }
    }
}
