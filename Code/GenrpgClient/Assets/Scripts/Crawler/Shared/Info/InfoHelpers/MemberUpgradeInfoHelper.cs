using Genrpg.Shared.Crawler.Upgrades.Settings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Spells.PlayerData.Spells;
using System.Collections.Generic;

namespace Genrpg.Shared.Crawler.Info.InfoHelpers
{
    public class MemberUpgradeInfoHelper : BaseInfoHelper<MemberUpgradeSettings,MemberUpgrade>
    {

        public override long Key => EntityTypes.MemberUpgrades;

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
            lines.Add(_infoService.CreateHeaderLine(upgrade.Name));
            lines.Add(upgrade.Desc);
            lines.Add("+" + upgrade.BonusPerTier + " bonus per tier.");
            lines.Add("You receive an upgrade point every " + settings.LevelsPerPoint);
            lines.Add("Max tier is: " + settings.MaxTier);
            return lines;
        }
    }
}
