using OxDb.SharedGame.Crawler.Crawlers.Services;
using OxDb.SharedGame.Crawler.Parties.PlayerData;

namespace OxDb.SharedGame.Crawler.States.StateHelpers.Combat
{
    public abstract class BaseCombatStateHelper : BaseStateHelper
    {
        public override bool HideBigPanels() { return false; }

        protected ICrawlerUpgradeService _roguelikeUpgradeService = null;

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


