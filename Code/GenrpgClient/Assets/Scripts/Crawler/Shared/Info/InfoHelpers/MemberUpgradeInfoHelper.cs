using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedGame.Crawler.Upgrades.Settings;
using System.Collections.Generic;

namespace OxDb.SharedGame.Crawler.Info.InfoHelpers
{
    public class MemberUpgradeInfoHelper : BaseInfoHelper<MemberUpgradeSettings, MemberUpgrade>
    {

        public override long HelperKey => EntityTypes.MemberUpgrades;

        public override List<string> GetInfoLines(long entityId)
        {


            MemberUpgradeSettings settings = _gameData.Get<MemberUpgradeSettings>(_gs.ch);
            MemberUpgrade upgrade = settings.Get(entityId);

            List<string> lines = new List<string>();

            if (upgrade == null)
            {
                lines.Add("Invalid Member Upgrade Type.");
                return lines;
            }
            lines.Add(_infoService.CreateHeaderLine(upgrade.Name, false));
            lines.Add(upgrade.Desc);
            lines.Add("+" + upgrade.BonusPerTier + " bonus per tier.");
            lines.Add("You receive an upgrade point every " + settings.LevelsPerPoint);
            lines.Add("Max tier is: " + settings.MaxTier);
            return lines;
        }
    }
}


