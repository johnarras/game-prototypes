using Genrpg.Shared.Crawler.Currencies.Settings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Stats.Settings.Stats;
using System.Collections.Generic;

namespace Genrpg.Shared.Crawler.Info.InfoHelpers
{
    public class CrawlerCurrencyTypeInfoHelper : BaseInfoHelper<CrawlerCurrencySettings, CrawlerCurrencyType>
    {

        public override long HelperKey => EntityTypes.CrawlerCurrency;

        protected override bool MakeEntityNamePlural() { return false; }

        public override List<string> GetInfoLines(long entityId)
        {
            List<string> lines = base.GetInfoLines(entityId);

            CrawlerCurrencyType ctype = _gameData.Get<CrawlerCurrencySettings>(_gs.ch).Get(entityId);

            if (ctype.CraftingStatTypeId > 0)
            {
                StatType statType = _gameData.Get<StatSettings>(_gs.ch).Get(ctype.CraftingStatTypeId);

                if (statType != null)
                {
                    lines.Add("Used to craft" + _infoService.CreateInfoLink(statType) + " items.");
                }
            }

            return lines;
        }
    }
}


