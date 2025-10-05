using Assets.Scripts.Crawler.Shared.Combat.Constants;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Crawler.Combat.Constants;
using Genrpg.Shared.Crawler.Combat.Entities;
using Genrpg.Shared.Crawler.Combat.Services;
using Genrpg.Shared.Crawler.Combat.Settings;
using Genrpg.Shared.Crawler.Monsters.Entities;
using Genrpg.Shared.Crawler.Monsters.Settings;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Roles.Constants;
using Genrpg.Shared.Crawler.Roles.Services;
using Genrpg.Shared.Crawler.Spells.Constants;
using Genrpg.Shared.Crawler.Spells.Entities;
using Genrpg.Shared.Crawler.Spells.Services;
using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Factions.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Spells.Constants;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.UnitEffects.Constants;
using Genrpg.Shared.Units.Settings;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.Crawler.Shared.Combat.Services
{
    public interface ISelectCombatActionsService : IInjectable
    {
        Task<ECombatStepResults> SetMonsterActions(PartyData party, CancellationToken token);
    }

    public class SelectCombatActionsService : ISelectCombatActionsService
    {
        private ICrawlerSpellService _crawlerSpellService = null;
        protected IGameData _gameData = null;
        protected IClientGameState _gs = null;
        protected IClientRandom _rand = null;
        private IRoleService _roleService = null;
        private ICrawlerCombatService _combatService = null;

        public async Task<ECombatStepResults> SetMonsterActions(PartyData party, CancellationToken token)
        {
            if (party.Combat.PartyGroup.CombatGroupAction != ECombatGroupActions.Advance &&
                (party.Combat == null || !_combatService.ReadyForCombat(party) || party.Combat.PartyGroup.CombatGroupAction == ECombatGroupActions.Prepare ||
                party.GetActiveParty().Any(x => x.Actions.Count < x.ActionsThisRound)))
            {
                return ECombatStepResults.End;
            }

            CrawlerCombatState combat = party.Combat;

            List<CrawlerUnit> tauntUnits = new List<CrawlerUnit>();
            List<CrawlerUnit> hiddenUnits = new List<CrawlerUnit>();
            List<CrawlerUnit> allUnits = new List<CrawlerUnit>();
            List<CrawlerUnit> nonGuardianPlayers = new List<CrawlerUnit>();

            foreach (CombatGroup combatGroup in combat.Allies)
            {
                List<CrawlerUnit> okUnits = combatGroup.Units.Where(x => !x.StatusEffects.HasBit(StatusEffects.Dead) &&
                !x.StatusEffects.HasBit(StatusEffects.Possessed)).ToList();

                tauntUnits.AddRange(okUnits.Where(x => x.DefendRank >= EDefendRanks.Guardian || !x.IsPlayer()));
                allUnits.AddRange(okUnits);
                hiddenUnits.AddRange(okUnits.Where(x => x.HideExtraRange > 0));
            }

            nonGuardianPlayers = allUnits.Where(x => x.IsPlayer()).Except(tauntUnits).Except(hiddenUnits).ToList();

            List<CrawlerUnit> monsterUnits = tauntUnits.Where(x => !x.IsPlayer()).ToList();

            if (monsterUnits.Count > 0 && !tauntUnits.Any(x => x.DefendRank == EDefendRanks.Taunt))
            {
                tauntUnits = monsterUnits;
            }

            List<CrawlerUnit> nonHiddenUnits = tauntUnits.Where(x => x.HideExtraRange == 0).ToList();
            if (nonHiddenUnits.Count > 0)
            {
                tauntUnits = nonHiddenUnits;
            }

            foreach (CombatGroup group in combat.Allies)
            {
                if (group != party.Combat.PartyGroup && party.Combat.PartyGroup.CombatGroupAction == ECombatGroupActions.Fight)
                {
                    SelectGroupActions(party, group, new List<CrawlerUnit>(), hiddenUnits, nonGuardianPlayers, combat.Allies, combat.Enemies);
                }
            }

            foreach (CombatGroup group in combat.Enemies)
            {
                SelectGroupActions(party, group, tauntUnits, hiddenUnits, nonGuardianPlayers, combat.Enemies, combat.Allies);
            }

            await Task.CompletedTask;
            return ECombatStepResults.Continue;
        }


        public void SelectGroupActions(PartyData party, CombatGroup group,
            List<CrawlerUnit> tauntUnits,
            List<CrawlerUnit> hiddenUnits,
            List<CrawlerUnit> nonGuardianPlayers,
            List<CombatGroup> friends,
            List<CombatGroup> foes)
        {
            CrawlerCombatSettings combatSettings = _gameData.Get<CrawlerCombatSettings>(_gs.ch);

            group.CombatGroupAction = ECombatGroupActions.None;
            if (group.Units.Count > 0 && group.Range > CrawlerCombatConstants.MinRange)
            {
                bool shouldAdvance = _rand.NextDouble() < combatSettings.GroupAdvanceChance;

                if (!shouldAdvance && group.UnitType != null)
                {
                    TribeType tribeType = _gameData.Get<TribeSettings>(_gs.ch).Get(group.UnitType.TribeTypeId);

                    if (!tribeType.HasRangedAttacks)
                    {
                        shouldAdvance = true;
                    }
                }


                if (shouldAdvance)
                {
                    group.CombatGroupAction = ECombatGroupActions.Advance;
                }
            }

            if (group.CombatGroupAction == ECombatGroupActions.None)
            {
                group.CombatGroupAction = ECombatGroupActions.Fight;

                List<CrawlerSpell> summonSpells = new List<CrawlerSpell>();
                List<CrawlerSpell> nonSummonSpells = new List<CrawlerSpell>();


                UnitType groupUnit = _gameData.Get<UnitTypeSettings>(_gs.ch).Get(group.UnitType.IdKey);

                if (groupUnit != null)
                {
                    List<long> spellIds = groupUnit.Effects.Where(x => x.EntityTypeId == EntityTypes.CrawlerSpell).Select(x => x.EntityId).ToList();
                    IReadOnlyList<CrawlerSpell> currentSpells = _gameData.Get<CrawlerSpellSettings>(_gs.ch).GetData().Where(x => spellIds.Contains(x.IdKey)).ToList();

                    summonSpells = currentSpells.Where(x => x.Effects.Any(e => e.EntityTypeId == EntityTypes.Unit && e.EntityId > 0)).ToList();
                    nonSummonSpells = currentSpells.Except(summonSpells).ToList();
                }

                foreach (CrawlerUnit unit in group.Units)
                {
                    SelectMonsterAction(party, group, unit, tauntUnits, hiddenUnits, nonGuardianPlayers, friends, foes, summonSpells, nonSummonSpells);
                }
            }
        }

        public void SelectMonsterAction(PartyData party, CombatGroup unitGroup,
            CrawlerUnit unit, List<CrawlerUnit> tauntUnits,
            List<CrawlerUnit> hiddenUnits,
            List<CrawlerUnit> nonGuardianPlayers,
            List<CombatGroup> allyGroups, List<CombatGroup> enemyGroups, List<CrawlerSpell> summonSpells,
            List<CrawlerSpell> nonSummonSpells)
        {
            CrawlerCombatSettings combatSettings = _gameData.Get<CrawlerCombatSettings>(_gs.ch);
            CrawlerMonsterSettings monsterSettings = _gameData.Get<CrawlerMonsterSettings>(_gs.ch);
            CrawlerSpellSettings spellSettings = _gameData.Get<CrawlerSpellSettings>(_gs.ch);


            if (party.Combat == null)
            {
                return;
            }

            if (unit.IsPlayer())
            {
                if (!unit.StatusEffects.HasBit(StatusEffects.Possessed))
                {
                    return;
                }
                else
                {
                    unit.ActionsThisRound = 1;
                    List<CombatGroup> temp = allyGroups;
                    allyGroups = enemyGroups;
                    enemyGroups = temp;
                    tauntUnits = new List<CrawlerUnit>();
                    return;
                }
            }
            else
            {
                unit.ActionsThisRound = 1;
            }

            double roleScalingValue = _roleService.GetRoleScalingLevel(party, unit, RoleScalingTypes.SpellDam);

            nonSummonSpells = nonSummonSpells.Where(x => x.RoleScalingTier <= roleScalingValue).ToList();

            List<CrawlerUnit> targets = new List<CrawlerUnit>();

            if (unit.FactionTypeId != FactionTypes.Player)
            {
                if (hiddenUnits.Count > 0 && _rand.Next() % 100 < unit.Stats.Max(StatTypes.DetectHidden))
                {
                    targets.AddRange(hiddenUnits);
                }
                else if (nonGuardianPlayers.Count > 0 && _rand.Next() % 100 < unit.Stats.Max(StatTypes.SmartTarget))
                {
                    targets.AddRange(nonGuardianPlayers);
                }
                else if (tauntUnits.Count > 0)
                {
                    targets.AddRange(tauntUnits);
                }
            }

            if (targets.Count < 1)
            {
                targets = SelectRandomGroupUnits(enemyGroups);
            }

            UnitAction combatAction = new UnitAction()
            {
                Caster = unit,
                FinalTargets = targets,
            };

            // Only enemy monsters summon in combat
            if (!allyGroups.Contains(party.Combat.PartyGroup) && summonSpells.Count > 0 && _rand.NextDouble() < combatSettings.SummonChance)
            {
                CrawlerSpell spell = summonSpells[_rand.Next(summonSpells.Count)];

                long cost = _crawlerSpellService.GetPowerCost(party, unit, spell);

                long mana = unit.Stats.Curr(StatTypes.Mana);

                if (mana >= cost)
                {
                    combatAction.CombatActionId = CombatActions.Cast;
                    combatAction.Spell = spell;
                    combatAction.FinalTargets = new List<CrawlerUnit>() { unit };
                }
            }

            if (combatAction.Spell == null && nonSummonSpells.Count > 0 && _rand.NextDouble() < combatSettings.CastSpellChance)
            {
                CrawlerSpell spell = nonSummonSpells[_rand.Next(nonSummonSpells.Count)];

                long cost = _crawlerSpellService.GetPowerCost(party, unit, spell);

                long mana = unit.Stats.Curr(StatTypes.Mana);

                if (mana >= cost)
                {
                    combatAction.CombatActionId = CombatActions.Cast;
                    combatAction.Spell = spell;
                    combatAction.FinalTargets = targets;

                    if (!_crawlerSpellService.IsEnemyTarget(spell.TargetTypeId))
                    {
                        if (spell.TargetTypeId == TargetTypes.AllAllies)
                        {
                            combatAction.FinalTargets = new List<CrawlerUnit>();
                            combatAction.FinalTargetGroups = new List<CombatGroup>(allyGroups);
                        }
                        else
                        {
                            combatAction.FinalTargets = new List<CrawlerUnit>() { unit };
                        }
                    }
                    else
                    {
                        if (spell.TargetTypeId == TargetTypes.OneEnemyGroup)
                        {
                            if (enemyGroups.Count > 0)
                            {
                                CombatGroup egroup = enemyGroups[_rand.Next(enemyGroups.Count)];
                                combatAction.FinalTargetGroups = new List<CombatGroup> { egroup };
                                //combatAction.FinalTargets = new List<CrawlerUnit>(egroup.Units);
                            }
                        }
                        else if (spell.TargetTypeId == TargetTypes.EnemyInEachGroup)
                        {
                            combatAction.FinalTargets = new List<CrawlerUnit>();
                            combatAction.FinalTargetGroups = new List<CombatGroup>(enemyGroups);
                        }
                    }
                }
            }

            // Now attack if we didn't cast a spell.
            if (combatAction.Spell == null)
            {
                combatAction.Spell = _gameData.Get<CrawlerSpellSettings>(_gs.ch).Get(CrawlerSpells.AttackId);
                combatAction.CombatActionId = CombatActions.Attack;
                if (unitGroup.Range > CrawlerCombatConstants.MinRange || !enemyGroups.Any(x => x.Range <= CrawlerCombatConstants.MinRange))
                {
                    combatAction.Spell = _gameData.Get<CrawlerSpellSettings>(_gs.ch).Get(CrawlerSpells.ShootId);
                    combatAction.CombatActionId = CombatActions.Shoot;
                }
            }

            if (combatAction.Spell != null)
            {
                unit.Actions.Clear();
                unit.AddAction(combatAction);
            }
        }

        private List<CrawlerUnit> SelectRandomGroupUnits(List<CombatGroup> groups)
        {
            List<CrawlerUnit> allUnits = new List<CrawlerUnit>();

            groups = groups.Where(x => x.Units.Any(u => !u.StatusEffects.HasBit(StatusEffects.Dead))).ToList();
            if (groups.Count > 0)
            {
                return groups[_rand.Next() % groups.Count].Units;
            }
            return null;
        }
    }
}
