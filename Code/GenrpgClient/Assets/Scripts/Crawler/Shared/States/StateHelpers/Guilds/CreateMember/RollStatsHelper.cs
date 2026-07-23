using OxDb.Client.UI.Constants;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Crawler.Options.Constants;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.Roles.Settings;
using OxDb.SharedGame.Crawler.States.Constants;
using OxDb.SharedGame.Crawler.States.Entities;
using OxDb.SharedGame.Crawler.Stats.Settings;
using OxDb.SharedGame.Stats.Constants;
using OxDb.SharedGame.Stats.Entities;
using OxDb.SharedGame.Stats.Settings.Stats;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.InputSystem;


namespace OxDb.SharedGame.Crawler.States.StateHelpers.Guilds.CreateMember
{
    public class RollStatsHelper : BaseStateHelper
    {

        public override ECrawlerStates HelperKey => ECrawlerStates.RollStats;

        public override async ValueTask<CrawlerStateData> Init(CrawlerStateData currentState, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            PartyMember member = action.ExtraData as PartyMember;

            PartyData party = _crawlerService.GetParty();

            CrawlerStatSettings statSettings = _gameData.Get<CrawlerStatSettings>(_gs.ch);

            member.ClearPermStats();
            member.Stats = new StatGroup();

            int startStatValue = statSettings.MinStartValue;

            bool rollStats = _optionsService.HasOption(party, CrawlerOptions.RollStats);

            List<Role> memberRoles = _gameData.Get<RoleSettings>(_gs.ch).GetRoles(member.Roles);

            List<StatType> statTypes = _gameData.Get<StatSettings>(_gs.ch).GetData()
                .Where(x => x.IdKey >= StatConstants.PrimaryStatStart &&
                x.IdKey <= StatConstants.PrimaryStatEnd).ToList();


            foreach (StatType statType in statTypes)
            {
                int statValue = RandUtils.IntRange(statSettings.MinStartValue, statSettings.MaxStartValue, _gs.Rand);

                if (!rollStats)
                {
                    statValue = startStatValue;
                }

                string textToShow = "";

                foreach (Role role in memberRoles)
                {
                    foreach (RoleBonusAmount amt in role.AmountBonuses)
                    {
                        if (amt.EntityTypeId == EntityTypes.StatBonus &&
                            amt.EntityId == statType.IdKey)
                        {
                            statValue += (int)amt.Amount;
                            textToShow += (amt.Amount > 0 ? "(+" : "(") + (int)amt.Amount + " " + role.Abbrev + ") ";
                        }
                    }
                }

                textToShow = statType.Name + ": " + statValue + " " + textToShow;

                member.AddPermStat(statType.IdKey, statValue);

                stateData.AddText(textToShow);
            }

            stateData.AddText(_textService.HighlightText($"Stat bonuses are applied to the initial stat" +
                " and per hit/tier when using a skill. ", TextColors.ColorGold));

            if (rollStats)
            {
                stateData.Actions.Add(new CrawlerStateAction("Reroll", Key.R, ECrawlerStates.RollStats, extraData: member));
            }
            else
            {
                stateData.AddBlankLine();
                stateData.AddBlankLine();

            }

            stateData.Actions.Add(new CrawlerStateAction("Accept", Key.A, ECrawlerStates.ChoosePortrait, extraData: member));


            stateData.Actions.Add(new CrawlerStateAction("Escape", Key.Escape, ECrawlerStates.ChooseClass,
                delegate
                {
                    member.Stats = new StatGroup();
                    while (member.Roles.Count > 1)
                    {
                        member.Roles.RemoveAt(1);
                    }

                }, member));

            await Task.CompletedTask;
            return stateData;

        }

    }
}


