using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedGame.Crawler.Info.InfoHelpers;
using OxDb.SharedGame.Trader.TradeGoods.Settings;
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


