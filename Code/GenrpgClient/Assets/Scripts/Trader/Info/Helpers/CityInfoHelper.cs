using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedGame.Crawler.Info.InfoHelpers;
using OxDb.SharedGame.Trader.Cities.Settings;
using System.Collections.Generic;

namespace OxDb.Client.Trader.Info.Helpers
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


