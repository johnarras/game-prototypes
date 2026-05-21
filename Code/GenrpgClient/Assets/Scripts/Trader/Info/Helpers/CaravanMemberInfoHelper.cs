using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedGame.Crawler.Info.InfoHelpers;
using OxDb.SharedGame.Trader.CaravanMembers.Settings;
using System.Collections.Generic;

namespace Assets.Scripts.Trader.Info.Helpers
{
    public class AnimalInfoHelper : BaseInfoHelper<CaravanMemberSettings, CaravanMember>
    {
        public override long HelperKey => EntityTypes.CaravanMember;

        public override List<string> GetInfoLines(long entityId)
        {
            List<string> lines = base.GetInfoLines(entityId);


            return lines;

        }
    }
}


