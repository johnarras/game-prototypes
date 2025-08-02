using Genrpg.Shared.Crawler.Crawlers.Services;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Services;
using Genrpg.Shared.Crawler.Upgrades.Settings;
using Genrpg.Shared.Entities.Constants;
using System.Collections.Generic;

namespace Genrpg.Shared.Crawler.Info.InfoHelpers
{
    public class PartyUpgradeInfoHelper : BaseInfoHelper<PartyUpgradeSettings,PartyUpgrade>
    {

        private ICrawlerUpgradeService _upgradeService = null;
        private ICrawlerService _crawlerService = null;

        public override long Key => EntityTypes.PartyUpgrades;


        public override List<string> GetInfoLines(long entityId)
        {

            PartyData party = _crawlerService.GetParty();

            PartyUpgrade upgrade = _gameData.Get<PartyUpgradeSettings>(_gs.ch).Get(entityId);

            int currTier = party.Upgrades.Get(upgrade.IdKey);
            int nextTier = currTier + 1;

            List<string> lines = new List<string>();

            lines.Add("You receive points every time you complete a dungeon level.");


            lines.Add(_infoService.CreateHeaderLine(upgrade.Name));

            lines.Add(upgrade.Desc);

            lines.Add("Bonus Per Tier: " + upgrade.BonusPerTier);

            lines.Add("Max Tier: " + upgrade.MaxTier);

            lines.Add("Tier: " + party.Upgrades.Get(upgrade.IdKey));

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
