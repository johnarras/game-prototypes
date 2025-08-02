using Genrpg.Shared.Crawler.Crawlers.Services;
using Genrpg.Shared.Crawler.Parties.PlayerData;

namespace Genrpg.Shared.Crawler.States.StateHelpers.Combat
{
    public abstract class BaseCombatStateHelper : BaseStateHelper
    {
        public override bool HideBigPanels() { return false; }
        protected ICrawlerUpgradeService _roguelikeUpgradeService;
        protected override CrawlerStateData CreateStateData()
        {
            CrawlerStateData stateData = base.CreateStateData();

            PartyData party = _crawlerService.GetParty();
            if (party.Combat != null && party.Combat.Enemies != null &&
                party.Combat.Enemies.Count > 0 &&
                party.Combat.Enemies[0].Units.Count > 0)
            {
                stateData.WorldSpriteName = null; // party.Combat.Enemies[0].Units[0].PortraitName;
            }

            return stateData;
        }
    }
}
