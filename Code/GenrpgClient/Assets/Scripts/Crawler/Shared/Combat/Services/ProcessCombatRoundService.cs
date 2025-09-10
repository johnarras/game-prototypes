using Assets.Scripts.Crawler.ClientEvents.ActionPanelEvents;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Crawler.Combat.Constants;
using Genrpg.Shared.Crawler.Combat.Entities;
using Genrpg.Shared.Crawler.Combat.Settings;
using Genrpg.Shared.Crawler.Monsters.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Roles.Settings;
using Genrpg.Shared.Crawler.Spells.Services;
using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Services;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.UnitEffects.Constants;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

        public async Task<bool> ProcessCombatRound(PartyData party, CancellationToken token)
        {
            if (party.Combat == null)
            {
                return false;
            }

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
                        return true;
                    }
                }
            }

            _combatService.SetInitialActions(party);

            // First order things.

            _combatService.SetMonsterActions(party);
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



            List<CrawlerUnit> allUnits = party.Combat.GetAllUnits();

            // Remove dead
            allUnits = allUnits.Where(x => !x.StatusEffects.HasBit(StatusEffects.Dead)).ToList();

            party.Combat.AttackSequence = SequenceUnitActionsByAscendingPriority(allUnits);

            while (party.Combat != null && party.Combat.AttackSequence.Count > 0)
            {
                CrawlerUnit unit = party.Combat.AttackSequence.Last();

                party.Combat.AttackSequence.Remove(unit);

                if (_combatService.IsDisabled(unit))
                {
                    continue;
                }

                if (unit.Action == null)
                {
                    continue;
                }

                if (unit.Action.IsComplete && unit.Action.SpellBeingCast != null &&
                    unit.Action.FinalTargets.Count > 0)
                {
                    await _spellService.CastSpellOnNextTarget(party, unit.Action, token);
                    continue;
                }
                else
                {
                    await _spellService.CastSpell(party, unit.Action, token);
                }
            }

            await _combatService.EndCombatRound(party);
            return true;
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
    }
}
