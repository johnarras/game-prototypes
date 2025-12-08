using Genrpg.Shared.Crawler.Info.InfoHelpers;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Trader.Cities.Settings;
using System.Collections.Generic;

namespace Assets.Scripts.Trader.Info.Helpers
{
    public class CityInfoHelper : BaseInfoHelper<CitySettings, City>
    {
        public override long HelperKey => EntityTypes.City;

        public override List<string> GetInfoLines(long entityId)
        {
            List<string> lines = base.GetInfoLines(entityId);


            return lines;

        }
    }
}
