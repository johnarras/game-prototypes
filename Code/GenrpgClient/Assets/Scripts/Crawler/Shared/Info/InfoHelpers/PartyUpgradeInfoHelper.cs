using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedGame.Crawler.Crawlers.Services;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.States.Services;
using OxDb.SharedGame.Crawler.Upgrades.Settings;
using System.Collections.Generic;

namespace OxDb.SharedGame.Crawler.Info.InfoHelpers
{
    public class PartyUpgradeInfoHelper : BaseInfoHelper<PartyUpgradeSettings, PartyUpgrade>
    {

        private ICrawlerUpgradeService _upgradeService = null;
        private ICrawlerService _crawlerService = null;

        public override long HelperKey => EntityTypes.PartyUpgrades;


        public override List<string> GetInfoLines(long entityId)
        {

            PartyData party = _crawlerService.GetParty();

            PartyUpgrade upgrade = _gameData.Get<PartyUpgradeSettings>(_gs.ch).Get(entityId);

            int currTier = party.Upgrades[upgrade.IdKey];
            int nextTier = currTier + 1;

            List<string> lines = new List<string>();

            lines.Add("You receive points every time you complete a dungeon level.");


            lines.Add(_infoService.CreateHeaderLine(upgrade.Name, false));

            lines.Add(upgrade.Desc);

            lines.Add("Bonus Per Tier: " + upgrade.BonusPerTier);

            lines.Add("Max Tier: " + upgrade.MaxTier);

            lines.Add("Tier: " + party.Upgrades[upgrade.IdKey]);

            lines.Add("Bonus: " + _upgradeService.GetPartyBonus(party, upgrade.IdKey));
            long nextUpgradeCost = _upgradeService.GetPartyUpgradeCost(upgrade.IdKey, nextTier);

            if (nextUpgradeCost > 0)
            {
                lines.Add("Next Tier Upgrade Cost: " + nextUpgradeCost);
                lines.Add("Your upgrade points: " + party.UpgradePoints);
            }

            return lines;
        }


    }
}


