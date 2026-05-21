
using Assets.Scripts.Awaitables;
using OxDb.SharedGame.Crawler.Combat.Constants;
using OxDb.SharedGame.Crawler.Monsters.Entities;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.States.Constants;
using OxDb.SharedGame.Crawler.States.Entities;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.InputSystem;

namespace OxDb.SharedGame.Crawler.States.StateHelpers.Combat
{
    public class CombatConfirmStateHelper : BaseCombatStateHelper
    {

        private IAwaitableService _awaitableService = null;

        public override ECrawlerStates HelperKey => ECrawlerStates.CombatConfirm;

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            PartyData party = _crawlerService.GetParty();

            if (party.Combat == null)
            {
                return new CrawlerStateData(ECrawlerStates.Error, true) { ExtraData = "Party is not in combat." };
            }

            if (party.Combat.PartyGroup.CombatGroupAction == ECombatGroupActions.Charge)
            {
                stateData.Actions.Add(new CrawlerStateAction("Are you sure you wish to charge the ranged units?", Key.None, ECrawlerStates.None));
            }
            else if (party.Combat.PartyGroup.CombatGroupAction == ECombatGroupActions.Run)
            {
                stateData.Actions.Add(new CrawlerStateAction("Are you sure you wish to run?", Key.None, ECrawlerStates.None));
            }
            else
            {
                foreach (CrawlerUnit combatUnit in party.Combat.PartyGroup.Units)
                {
                    string text = combatUnit.Name + ": ";
                    for (int a = 0; a < combatUnit.CombatActions.Count; a++)
                    {
                        text += combatUnit.CombatActions[a].Text;
                        if (a < combatUnit.CombatActions.Count - 1)
                        {
                            text += " and ";
                        }
                    }
                    stateData.AddText(text);
                }
                stateData.Actions.Add(new CrawlerStateAction("Use these actions?", Key.None, ECrawlerStates.None));
            }

            stateData.Actions.Add(new CrawlerStateAction("Yes", Key.Y, ECrawlerStates.ProcessCombatRound));
            stateData.Actions.Add(new CrawlerStateAction("No", Key.N, ECrawlerStates.CombatFightRun,
                onClickAction: delegate ()
                {
                    // Need to reset all combat round data and start over.
                    _awaitableService.ForgetTask(_combatService.EndCombatRound(party, token));
                }));


            await Task.CompletedTask;
            return stateData;


        }
    }
}


