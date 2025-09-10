using Assets.Scripts.Awaitables;
using Assets.Scripts.Crawler.ClientEvents.ActionPanelEvents;
using Assets.Scripts.Crawler.Constants;
using Genrpg.Shared.Crawler.Combat.Services;
using Genrpg.Shared.Crawler.Loot.Services;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Party.Services;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Utils;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;


namespace Genrpg.Shared.Crawler.States.StateHelpers.Combat
{
    public class ProcessCombatRoundStateHelper : BaseCombatStateHelper
    {
        private IPartyService _partyService = null;
        private IProcessCombatRoundCombatService _processCombatService = null;
        private IAwaitableService _awaitableService = null;
        private ILootGenService _lootGenService = null;

        public override ECrawlerStates Key => ECrawlerStates.ProcessCombatRound;

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            stateData.Actions.Add(new CrawlerStateAction("Processing combat...\n\n"));
            stateData.Actions.Add(new CrawlerStateAction("FIGHT!"));
            stateData.Actions.Add(new CrawlerStateAction("", CharCodes.Space, ECrawlerStates.DoNotChangeState, ContinueAtEndOfRound));
            stateData.Actions.Add(new CrawlerStateAction("", 'F', ECrawlerStates.DoNotChangeState, ContinueAtEndOfRound));

            PartyData party = _crawlerService.GetParty();

            if (!_combatService.ReadyForCombat(party))
            {
                await _combatService.EndCombatRound(party);
                stateData = new CrawlerStateData(ECrawlerStates.CombatFightRun, true);
            }

            _awaitableService.ForgetAwaitable(ProcessCombat(party, token));

            await Task.CompletedTask;
            return stateData;
        }

        private bool _canEndCombatRound = false;
        private bool _processingCombat = false;
        private async Awaitable ProcessCombat(PartyData party, CancellationToken token)
        {
            try
            {
                _canEndCombatRound = false;
                _processingCombat = true;
                await Task.Delay(100, token);
                bool success = await _processCombatService.ProcessCombatRound(party, token);
                _processingCombat = false;
                _dispatcher.Dispatch(new AddActionPanelText($"\n\nPress {_textService.HighlightText("Space")} OR " +
                    $"{_textService.HighlightText("F")} to continue...\n\n",
                    ContinueAtEndOfRound));

                for (int i = 0; i < 1; i++)
                {
                    await Task.Delay(10, token);
                    _dispatcher.Dispatch(new AddActionPanelText("\n"));
                }

                while (!_canEndCombatRound)
                {
                    if (party.Combat.PartyWonCombat())
                    {
                        _canEndCombatRound = true;
                    }

                    await Task.Delay(10, token);
                }

                if (!success || party.Combat == null)
                {
                    _crawlerService.ChangeState(ECrawlerStates.ExploreWorld, token);
                }
                else
                {
                    if (party.Combat.PartyWonCombat())
                    {
                        LootGenData lootGenData = await _lootGenService.GenerateCombatLoot(party, token);
                        _combatService.EndCombat(party);
                        await Awaitable.WaitForSecondsAsync(CrawlerClientCombatConstants.GiveCombatLootDelaySeconds, token);
                        _crawlerService.ChangeState(ECrawlerStates.GiveLoot, token, lootGenData);
                    }
                    else if (!(await _partyService.CheckIfPartyIsDead(party, token)))
                    {
                        _crawlerService.ChangeState(ECrawlerStates.CombatFightRun, token);
                    }
                }
            }
            catch (Exception e)
            {
                _logService.Exception(e, "ProcessCombatRound");
            }
        }

        private void ContinueAtEndOfRound()
        {
            if (!_processingCombat)
            {
                _canEndCombatRound = true;
            }
        }
    }
}
