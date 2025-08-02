using Assets.Scripts.ClientEvents;
using Assets.Scripts.UI.Constants;
using Genrpg.Shared.Crawler.Crawlers.Services;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Crawler.Upgrades.Settings;
using Genrpg.Shared.UI.Constants;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Crawler.States.StateHelpers.Guilds.Upgrades
{
    public class UpgradePartyStateHelper : BaseStateHelper
    {

        private ICrawlerUpgradeService _upgradeService;
        public override ECrawlerStates Key => ECrawlerStates.UpgradeParty;

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {


            await Task.CompletedTask;
            CrawlerStateData stateData = CreateStateData();

            string oldErrorText = action.ExtraData as String;


            PartyData party = _crawlerService.GetParty();

            stateData.AddText("Crawler Upgrades:\n");
            if (!string.IsNullOrEmpty(oldErrorText))
            {
                stateData.Actions.Add(new CrawlerStateAction(_textService.HighlightText(oldErrorText, TextColors.ColorRed)));
            }
            else
            {
                stateData.Actions.Add(new CrawlerStateAction(" \n", forceText:true));
            }

            string errorText = "";

            stateData.Actions.Add(new CrawlerStateAction($"Reset Points Current: {party.UpgradePoints}, Total: {party.TotalUpgradePoints}", CharCodes.None, ECrawlerStates.UpgradeParty,
                () => 
                { 
                    _upgradeService.ResetPartyUpgradePoints(party); 

                }, errorText, forceText: true));

            PartyUpgradeSettings settings = _gameData.Get<PartyUpgradeSettings>(_gs.ch);

            StringBuilder sb = new StringBuilder();
            foreach (PartyUpgrade upgrade in settings.GetData())
            {
                sb.Clear();

                int currTier = party.Upgrades.Get(upgrade.IdKey); 
                int nextTier = currTier + 1;

                sb.Append(upgrade.Name + "[T" + party.Upgrades.Get(upgrade.IdKey) + " +" + _upgradeService.GetPartyBonus(party, upgrade.IdKey) + "]");

                long nextUpgradeCost = _upgradeService.GetPartyUpgradeCost(upgrade.IdKey, nextTier);

                if (nextUpgradeCost > 0)
                {
                    sb.Append(" N: [$" + nextUpgradeCost);
                    sb.Append(" +" + _upgradeService.GetPartyBonus(party, upgrade.IdKey, nextTier) + "]");
                }

                stateData.Actions.Add(new CrawlerStateAction(sb.ToString(), CharCodes.None, ECrawlerStates.UpgradeParty,
                    () =>
                    {
                        if (party.Members.Count > 0)
                        {

                        }
                        _upgradeService.PayForPartyUpgrade(party, upgrade.IdKey);

                    }, errorText, null,  () => ShowUpgradeTooltop(upgrade.IdKey)));
            }


            stateData.Actions.Add(new CrawlerStateAction("Escape", CharCodes.Escape, ECrawlerStates.GuildMain));
            return stateData;
        }


        private void ShowUpgradeTooltop(long roguelikeUpgradeId)
        {
            List<string> allLines = new List<string>();

            PartyUpgrade upgrade = _gameData.Get<PartyUpgradeSettings>(_gs.ch).Get(roguelikeUpgradeId);

            if (upgrade == null)
            {
                return;
            }

            allLines.Add("Upgrade: " + upgrade.Name + "\n\n");

            allLines.Add(upgrade.Desc + "\n\n");

            allLines.Add("Base Upgrade Cost: " + upgrade.BasePointCost + "\n\n");

            allLines.Add("Total upgrade cost is NewTier*BaseUpgradeCost\n\n");

            allLines.Add("Bonus Per Tier: " + upgrade.BonusPerTier +"\n\n");

            allLines.Add("Max Tier: " + upgrade.MaxTier + "\n\n");  

            _dispatcher.Dispatch(new ShowInfoPanelEvent() { Lines = allLines });
        }
    }
}
