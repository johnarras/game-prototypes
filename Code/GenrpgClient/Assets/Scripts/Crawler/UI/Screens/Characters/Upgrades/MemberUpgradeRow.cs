using Assets.Scripts.Crawler.UI.WorldUI;
using Genrpg.Shared.Crawler.Upgrades.Settings;

namespace Assets.Scripts.Crawler.UI.Screens.Characters.Upgrades
{
    public class MemberUpgradeRow : RolloverInfoRow
    {

        private MemberUpgrade _upgrade = null;

        public void SetData(MemberUpgrade upgrade, long tier, double value)
        {
            _upgrade = upgrade;
            _uiService.SetText(MainText, _infoService.CreateInfoLink(upgrade) + "(" + tier + ") " + (value >= 0 ? "+" : "") + value);
        }
    }
}


