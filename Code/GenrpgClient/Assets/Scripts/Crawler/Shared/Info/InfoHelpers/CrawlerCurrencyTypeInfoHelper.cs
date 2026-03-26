using Genrpg.Shared.Currencies.Settings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Stats.Settings.Stats;
using System.Collections.Generic;

namespace Genrpg.Shared.Crawler.Info.InfoHelpers
{
    public class CoreCurrencyTypeInfoHelper : BaseInfoHelper<CoreCurrencyTypeSettings, CoreCurrencyType>
    {

        public override long HelperKey => EntityTypes.CoreCurrency;

        protected override bool MakeEntityNamePlural() { return false; }

        public override List<string> GetInfoLines(long entityId)
        {
            List<string> lines = base.GetInfoLines(entityId);

            CoreCurrencyType ctype = _gameData.Get<CoreCurrencyTypeSettings>(_gs.ch).Get(entityId);

            if (ctype.StatTypeId > 0)
            {
                StatType statType = _gameData.Get<StatSettings>(_gs.ch).Get(ctype.StatTypeId);

                if (statType != null)
                {
                    lines.Add("Used to craft" + _infoService.CreateInfoLink(statType) + " items.");
                }
            }

            return lines;
        }
    }
}


