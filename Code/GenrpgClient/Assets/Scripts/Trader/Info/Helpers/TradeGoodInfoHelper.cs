using Genrpg.Shared.Crawler.Info.InfoHelpers;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Trader.TradeGoods.Settings;
using System.Collections.Generic;

namespace Assets.Scripts.Trader.Info.Helpers
{
    public class TradeGoodInfoHelper : BaseInfoHelper<TradeGoodSettings, TradeGood>
    {
        public override long HelperKey => EntityTypes.TradeGood;

        public override List<string> GetInfoLines(long entityId)
        {
            List<string> lines = base.GetInfoLines(entityId);


            return lines;

        }
    }
}
