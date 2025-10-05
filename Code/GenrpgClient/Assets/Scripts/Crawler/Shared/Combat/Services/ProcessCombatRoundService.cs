using Assets.Scripts.Crawler.ClientEvents.ActionPanelEvents;
using Assets.Scripts.Crawler.Shared.Combat.Constants;
using Assets.Scripts.Crawler.Shared.Combat.Services;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Crawler.Combat.Constants;
using Genrpg.Shared.Crawler.Combat.Entities;
using Genrpg.Shared.Crawler.Combat.Settings;
using Genrpg.Shared.Crawler.Monsters.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Roles.Settings;
using Genrpg.Shared.Crawler.Spells.Entities;
using Genrpg.Shared.Crawler.Spells.Services;
using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Services;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.UnitEffects.Constants;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Genrpg.Shared.Crawler.Combat.Services
{
    public interface IProcessCombatRoundCombatService : IInjectable
    {
        Task<bool> ProcessCombatRound(PartyData party, CancellationToken token);
    }
    public class ProcessCombatRoundCombatService : IProcessCombatRoundCombatService
    {
        private ICrawlerSpellService _spellService = null;
        private ICrawlerCombatService _combatService = null;
        private ICrawlerService _crawlerService = null;
        protected IClientGameState _gs = null;
        protected IClientRandom _rand = null;
        private IGameData _gameData = null;
        private IDispatcher _dispatcher = null;
        private ISelectCombatActionsService _selectActionService = null;


        private List<Func<PartyData, CancellationToken, Task<ECombatStepResults>>> _steps = new List<Func<PartyData, CancellationToken, Task<ECombatStepResults>>>()
        {
        };


        public async Task<bool> ProcessCombatRound(PartyData party, CancellationToken token)
        {
            if (party.Combat == null)
            {
                return false;
            }

            if (_steps.Count < 1)
            {
                _steps.Add(PartyRanFromCombat);
                _steps.Add(SetPreCombatConditions);
                _steps.Add(_selectActionService.SetMonsterActions);
                _steps.Add(AdvanceGroups);
                _steps.Add(CreateInitialCombatActionSequence);
                _steps.Add(ProcessAllCombatants);
                _steps.Add(_combatService.EndCombatRound);
            }


            foreach (Func<PartyData, CancellationToken, Task<ECombatStepResults>> step in _steps)
            {
                if (await step(party, token) == ECombatStepResults.End)
                {
                    return false;
                }
            }


            return true;
        }


        private async Task<ECombatStepResults> ProcessAllCombatants(PartyData party, CancellationToken token)
        {
            while (party.Combat != null && party.Combat.AttackSequence.Count > 0)
            {
                CrawlerUnit unit = party.Combat.AttackSequence.Last();

                party.Combat.AttackSequence.Remove(unit);

                if (_combatService.IsDisabled(unit))
                {
                    continue;
                }

                if (unit.Actions.Count < 1)
                {
                    continue;
                }

                UnitAction currentAction = unit.Actions.FirstOrDefault(x => x.DidCast && x.SpellBeingCast != null &&
                x.SpellBeingCast.HitsLeft > 0 &&
                x.FinalTargets.Count > 0);

                if (currentAction == null)
                {
                    unit.Actions = unit.Actions.Where(x => !x.DidCast && x.SpellBeingCast == null && x.Spell != null).ToList();

                    if (unit.Actions.Count > 0)
                    {
                        await _spellService.CastSpell(party, unit.Actions[0], token);
                    }
                }
                else
                {
                    await _spellService.CastSpellOnNextTarget(party, currentAction, token);

                }
            }
            return ECombatStepResults.Continue;
        }

        private async Task<ECombatStepResults> CreateInitialCombatActionSequence(PartyData party, CancellationToken token)
        {
            List<CrawlerUnit> allUnits = party.Combat.GetAllUnits();

            // Remove dead
            allUnits = allUnits.Where(x => !x.StatusEffects.HasBit(StatusEffects.Dead)).ToList();

            party.Combat.AttackSequence = SequenceUnitActionsByAscendingPriority(allUnits);
            await Task.CompletedTask;
            return ECombatStepResults.Continue;
        }

        private async Task<ECombatStepResults> AdvanceGroups(PartyData party, CancellationToken token)
        {
            if (party.Combat.PartyGroup.CombatGroupAction == ECombatGroupActions.Advance)
            {
                CrawlerSpell chargeSpell = _gameData.Get<CrawlerSpellSettings>(_gs.ch).GetData().FirstOrDefault(x => x.Name == "Charge");

                int advanceRange = CrawlerCombatConstants.RangeDelta;

                if (chargeSpell != null)
                {

                    RoleSettings roleSettings = _gameData.Get<RoleSettings>(_gs.ch);

                    int chargeCharacters = 0;

                    List<PartyMember> activeParty = party.GetActiveParty();

                    foreach (PartyMember member in activeParty)
                    {
                        if (roleSettings.HasBonus(member.Roles, EntityTypes.CrawlerSpell, chargeSpell.IdKey))
                        {
                            chargeCharacters++;
                            break;
                        }
                    }

                    advanceRange *= 1 + chargeCharacters;
                }

                foreach (CombatGroup group in party.Combat.Enemies)
                {
                    if (group.Range > CrawlerCombatConstants.MinRange)
                    {
                        // Yes this can compress groups that are really far away, feels more rewarding
                        // even if it's weird that spread out groups get piled on top of each other.
                        group.Range = Math.Max(CrawlerCombatConstants.MinRange, group.Range - advanceRange);
                    }
                }
                _dispatcher.Dispatch(new AddActionPanelText($"You Advance. {advanceRange}'."));
            }

            foreach (CombatGroup group in party.Combat.Enemies)
            {
                if (group.CombatGroupAction == ECombatGroupActions.Advance)
                {
                    if (group.Range > CrawlerCombatConstants.MinRange)
                    {
                        group.Range -= CrawlerCombatConstants.RangeDelta;
                    }
                    _dispatcher.Dispatch(new AddActionPanelText($"Group of {group.PluralName} Advances {CrawlerCombatConstants.MinRange}"));
                    _dispatcher.Dispatch(new AddActionPanelText(_combatService.ShowGroupStatus(group)));
                }
            }
            await Task.CompletedTask;
            return ECombatStepResults.Continue;
        }

        private async Task<ECombatStepResults> PartyRanFromCombat(PartyData party, CancellationToken token)
        {

            if (party.Combat.PartyGroup.CombatGroupAction == ECombatGroupActions.Run)
            {
                long totalLuck = party.Combat.PartyGroup.Units.Sum(x => x.Stats.Max(StatTypes.Luck));
                if (totalLuck > 0)
                {
                    double averageLuck = 1.0 * totalLuck / party.Combat.PartyGroup.Units.Count;

                    if (_rand.NextDouble() * party.Combat.Level < averageLuck)
                    {
                        _combatService.EndCombat(party);
                        _crawlerService.ChangeState(ECrawlerStates.ExploreWorld, token);
                        return ECombatStepResults.End;
                    }
                }
            }
            await Task.CompletedTask;
            return ECombatStepResults.Continue;
        }


        private List<CrawlerUnit> SequenceUnitActionsByAscendingPriority(List<CrawlerUnit> allUnits)
        {

            CrawlerCombatSettings combatSettings = _gameData.Get<CrawlerCombatSettings>(_gs.ch);
            int speedDeltaPercent = combatSettings.SpeedCombatSequencingDeltaPercent;

            // Descending by speed.
            foreach (CrawlerUnit unit in allUnits)
            {
                unit.CombatPriority = unit.Stats.Max(StatTypes.Speed) * MathUtils.FloatRange(1 - speedDeltaPercent, 1 + speedDeltaPercent, _rand);
            }

            allUnits = allUnits.OrderBy(x => x.CombatPriority).ToList();

            foreach (CrawlerUnit unit in allUnits)
            {
                if (unit.StatusEffects.HasBit(StatusEffects.Slowed))
                {
                    unit.CombatPriority *= combatSettings.SlowEffectPriorityScale;
                }
            }
            return allUnits;
        }


        public async Task<ECombatStepResults> SetPreCombatConditions(PartyData party, CancellationToken token)
        {
            // Pass 1 defend and hide


            List<long> defenderRoleIds = _gameData.Get<RoleSettings>(_gs.ch).GetData().Where(x => x.Guardian).Select(x => x.IdKey).ToList();

            foreach (CrawlerUnit unit in party.Combat.PartyGroup.Units)
            {
                if (party.Combat.PartyGroup.Units.Any(x => x.Actions.Count < x.ActionsThisRound ||
                !unit.Actions.Any(x => x.DidCast)))
                {
                    continue;
                }


                unit.DefendRank = EDefendRanks.None;

                foreach (UnitRole unitRole in unit.Roles)
                {
                    if (defenderRoleIds.Contains(unitRole.RoleId))
                    {
                        unit.DefendRank = EDefendRanks.Guardian;
                        unit.IsGuardian = true;
                        break;
                    }
                }

                if (unit.Actions.Any(x => x.CombatActionId == CombatActions.Defend))
                {
                    if (unit.DefendRank == EDefendRanks.Guardian)
                    {
                        unit.DefendRank = EDefendRanks.Taunt;
                    }
                    else
                    {
                        unit.DefendRank = EDefendRanks.Defend;
                    }
                }
                else if (unit.Actions.Any(x => x.CombatActionId == CombatActions.Hide))
                {
                    unit.HideExtraRange += CrawlerCombatConstants.RangeDelta;
                }
            }

            foreach (CombatGroup cgroup in party.Combat.Allies)
            {
                if (cgroup == party.Combat.PartyGroup)
                {
                    continue;
                }

                foreach (CrawlerUnit unit in cgroup.Units)
                {
                    if (unit.IsGuardian)
                    {
                        unit.DefendRank = EDefendRanks.Guardian;
                    }
                }
            }

            await Task.CompletedTask;
            return ECombatStepResults.Continue;
        }
    }
}
