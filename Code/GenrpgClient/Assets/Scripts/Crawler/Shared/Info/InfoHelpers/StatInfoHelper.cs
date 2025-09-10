using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.Stats.Settings.Stats;
using System;
using System.Collections.Generic;

namespace Genrpg.Shared.Crawler.Info.InfoHelpers
{
    public class StatInfoHelper : BaseInfoHelper<StatSettings, StatType>
    {

        public override long Key => EntityTypes.Stat;

        protected override bool IsValidInfoChild(StatType stype)
        {
            return stype.IsCrawlerStat;
        }

        public override List<string> GetInfoLines(long entityId)
        {

            List<String> lines = new List<string>();
            StatSettings statSettings = _gameData.Get<StatSettings>(_gs.ch);

            StatType stype = statSettings.Get(entityId);

            if (stype == null)
            {
                return lines;
            }

            lines.Add(_infoService.CreateHeaderLine(stype.Name));

            lines.Add("");
            lines.Add(stype.Desc);


            if (stype.IdKey >= StatConstants.PrimaryStatStart && stype.IdKey <= StatConstants.PrimaryStatEnd)
            {
                lines.Add("");

                lines.Add("If your stat is at least 16");
                lines.Add("your stat bonus for this stat is\n(statVal-15)^(2/3)");
            }

            if (stype.BonusStatTypeId > 0)
            {
                StatType otherStatType = statSettings.Get(stype.BonusStatTypeId);

                if (otherStatType != null)
                {
                    lines.Add("");
                    lines.Add($"Your Stat Bonus for this stat is applied as");
                    lines.Add($" a percentage bonus to {_infoService.CreateInfoLink(otherStatType)}");
                }
            }

            return lines;

        }
    }
}
