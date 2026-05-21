using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedGame.Currencies.Settings;
using OxDb.SharedGame.Stats.Settings.Stats;
using System.Collections.Generic;

namespace OxDb.SharedGame.Crawler.Info.InfoHelpers
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
                    lines.Add("Used to craft " + _infoService.CreateInfoLink(statType) + " items.");
                }
            }

            return lines;
        }
    }
}


