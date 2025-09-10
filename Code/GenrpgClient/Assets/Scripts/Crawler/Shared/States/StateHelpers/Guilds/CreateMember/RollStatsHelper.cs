using Assets.Scripts.UI.Constants;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Roles.Settings;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Crawler.Stats.Settings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.Stats.Entities;
using Genrpg.Shared.Stats.Settings.Stats;
using Genrpg.Shared.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace Genrpg.Shared.Crawler.States.StateHelpers.Guilds.CreateMember
{
    public class RollStatsHelper : BaseStateHelper
    {

        public override ECrawlerStates Key => ECrawlerStates.RollStats;

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentState, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            PartyMember member = action.ExtraData as PartyMember;

            PartyData party = _crawlerService.GetParty();

            CrawlerStatSettings statSettings = _gameData.Get<CrawlerStatSettings>(_gs.ch);

            member.ClearPermStats();
            member.Stats = new StatGroup();

            int startStatValue = statSettings.StartStat;

            List<Role> memberRoles = _gameData.Get<RoleSettings>(_gs.ch).GetRoles(member.Roles);

            List<StatType> statTypes = _gameData.Get<StatSettings>(_gs.ch).GetData()
                .Where(x => x.IdKey >= StatConstants.PrimaryStatStart &&
                x.IdKey <= StatConstants.PrimaryStatEnd).ToList();

            foreach (StatType statType in statTypes)
            {
                int statValue = startStatValue + MathUtils.IntRange(statSettings.MinRollValue, statSettings.MaxRollValue, _rand);

                string textToShow = "";

                foreach (Role role in memberRoles)
                {
                    foreach (RoleBonusAmount amt in role.AmountBonuses)
                    {
                        if (amt.EntityTypeId == EntityTypes.StatBonus &&
                            amt.EntityId == statType.IdKey)
                        {
                            statValue += (int)amt.Amount;
                            textToShow += (amt.Amount > 0 ? "(+" : "(") + (int)amt.Amount + " " + role.Name + ") ";
                        }
                    }
                }

                textToShow = statType.Name + ": " + statValue + " " + textToShow;



                member.AddPermStat(statType.IdKey, statValue);

                stateData.AddText(textToShow);
            }

            stateData.AddText(_textService.HighlightText($"Stat bonuses are applied to the initial stat" +
                " and per action/tier whenever a skill " +
                "using that stat is used. So a +2 strength bonus with"
                + " 5 melee attacks would apply 2 damage to each attack.", TextColors.ColorGold));

            if (statSettings.MinRollValue < statSettings.MaxRollValue)
            {
                stateData.Actions.Add(new CrawlerStateAction("Reroll", 'R', ECrawlerStates.RollStats, extraData: member));
            }
            else
            {
                stateData.AddBlankLine();
                stateData.AddBlankLine();

            }

            stateData.Actions.Add(new CrawlerStateAction("Accept", 'A', ECrawlerStates.ChoosePortrait, extraData: member));


            stateData.Actions.Add(new CrawlerStateAction("Escape", CharCodes.Escape, ECrawlerStates.ChooseClass,
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
