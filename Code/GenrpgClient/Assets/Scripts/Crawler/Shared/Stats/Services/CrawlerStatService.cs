using Assets.Scripts.Crawler.ClientEvents.StatusPanelEvents;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Crawler.Combat.Settings;
using Genrpg.Shared.Crawler.Crawlers.Services;
using Genrpg.Shared.Crawler.Monsters.Entities;
using Genrpg.Shared.Crawler.Monsters.Settings;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Roles.Constants;
using Genrpg.Shared.Crawler.Roles.Settings;
using Genrpg.Shared.Crawler.Stats.Settings;
using Genrpg.Shared.Crawler.Training.Settings;
using Genrpg.Shared.Crawler.Upgrades.Constants;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Factions.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.Stats.Entities;
using Genrpg.Shared.Stats.Settings.Stats;
using Genrpg.Shared.UnitEffects.Constants;
using Genrpg.Shared.UnitEffects.Settings;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Units.Settings;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Genrpg.Shared.Crawler.Stats.Services
{
    public interface ICrawlerStatService : IInjectable
    {
        void CalcUnitStats(PartyData party, CrawlerUnit unit, bool resetCurrStats);

        void CalcPartyStats(PartyData party, bool resetCurrStats);

        long GetStatBonus(PartyData party, CrawlerUnit unit, long statId);

        void Add(PartyData party, CrawlerUnit unit, long statTypeId, int statCategory, long value, long elementTypeId = 0);
    }



    public class CrawlerStatService : ICrawlerStatService
    {
        protected IStatService _statService = null;
        protected IGameData _gameData = null;
        protected IClientGameState _gs = null;
        protected IClientRandom _rand = null;
        private ICrawlerUpgradeService _upgradeService = null;
        private IDispatcher _dispatcher = null;

        public void CalcPartyStats(PartyData party, bool resetCurrStats)
        {
            foreach (PartyMember member in party.Members)
            {
                if (member.PartySlot > 0)
                {
                    CalcUnitStats(party, member, resetCurrStats);
                }
            }
        }

        public void CalcUnitStats(PartyData party, CrawlerUnit unit, bool resetCurrStats)
        {
            if (unit.Level < 1)
            {
                unit.Level = 1;
            }

            RoleSettings roleSettings = _gameData.Get<RoleSettings>(_gs.ch);
            CrawlerCombatSettings combatSettings = _gameData.Get<CrawlerCombatSettings>(_gs.ch);
            CrawlerMonsterSettings monsterSettings = _gameData.Get<CrawlerMonsterSettings>(_gs.ch);
            CrawlerStatSettings statSettings = _gameData.Get<CrawlerStatSettings>(_gs.ch);
            CrawlerTrainingSettings trainingSettings = _gameData.Get<CrawlerTrainingSettings>(_gs.ch);

            IReadOnlyList<StatType> allStats = _gameData.Get<StatSettings>(_gs.ch).GetData();

            IReadOnlyList<Role> allRoles = roleSettings.GetData();

            List<long> buffStatTypes = new List<long>();

            foreach (Role role in allRoles)
            {
                buffStatTypes.AddRange(role.BinaryBonuses.Where(x => x.EntityTypeId == EntityTypes.Stat).Select(x => x.EntityId));
            }

            List<Role> classRoles = allRoles.Where(x => x.RoleCategoryId == RoleCategories.Class).ToList();
            List<Role> raceRoles = allRoles.Where(x=>x.RoleCategoryId == RoleCategories.Origin).ToList();

            buffStatTypes = buffStatTypes.Where(x => x < StatConstants.PrimaryStatStart || x > StatConstants.PrimaryStatEnd).ToList();

            buffStatTypes = buffStatTypes.Distinct().ToList();

            List<long> mutableStatTypes = new List<long>() { StatTypes.Health, StatTypes.Mana };

            List<StatVal> currStats = new List<StatVal>();

            if (unit is PartyMember member)
            {
                List<Role> roles = _gameData.Get<RoleSettings>(_gs.ch).GetRoles(member.Roles);

                foreach (long mutableStatType in mutableStatTypes)
                {
                    currStats.Add(new StatVal()
                    {
                        StatTypeId = (short)mutableStatType,
                        Val = (int)member.Stats.Curr(mutableStatType),
                    });
                }

                member.Stats.ResetAll();

                long bonusPercent = (long)_upgradeService.GetPartyBonus(party, PartyUpgrades.StatPercent);

                for (int primaryStatId = StatConstants.PrimaryStatStart; primaryStatId < StatConstants.PrimaryStatEnd; primaryStatId++)
                {
                    _statService.Add(member, primaryStatId, StatCategories.Base, member.GetPermStat(primaryStatId));
                    _statService.Add(member, primaryStatId, StatCategories.Pct, bonusPercent);
                }

                foreach (long buffStatType in buffStatTypes)
                {

                    _statService.Set(member, buffStatType, StatCategories.Base, statSettings.BaseBuffStatValue + member.Level);

                }

                // Now do equipment.

                foreach (Item item in member.Equipment)
                {
                    foreach (ItemEffect eff in item.Effects)
                    {
                        if (eff.EntityTypeId == EntityTypes.Stat)
                        {
                            _statService.Add(member, eff.EntityId, StatCategories.Bonus, eff.Quantity);
                        }
                    }
                }

                long totalHealth = unit.Level * GetStatBonus(party, member, StatTypes.Stamina);
                long totalMana = unit.Level * GetStatBonus(party, member, StatTypes.Willpower);

                foreach (Role role in roles)
                {
                    UnitRole unitRole = unit.Roles.FirstOrDefault(x => x.RoleId == role.IdKey);

                    if (unitRole != null)
                    {
                        totalHealth += unitRole.Level * role.HealthPerLevel;
                        totalMana += unitRole.Level * role.ManaPerLevel;
                    }
                }

                _statService.Set(member, StatTypes.Health, StatCategories.Base, totalHealth);
                _statService.Set(member, StatTypes.Mana, StatCategories.Base, totalMana);

                foreach (long mutableStatType in mutableStatTypes)
                {
                    long currStatVal = currStats.FirstOrDefault(x => x.StatTypeId == mutableStatType).Val;
                    long maxStatVal = member.Stats.Max(mutableStatType);

                    if (resetCurrStats || currStatVal > maxStatVal)
                    {
                        _statService.Set(member, mutableStatType, StatCategories.Curr, maxStatVal);
                    }
                    else
                    {
                        _statService.Set(member, mutableStatType, StatCategories.Curr, currStatVal);
                    }
                }

                // Now give bonus stats.
                foreach (StatType stype in allStats)
                {
                    if (stype.BonusStatTypeId > 0)
                    {
                        _statService.Add(unit, stype.BonusStatTypeId, StatCategories.Pct, GetStatBonus(party, member, stype.IdKey));
                    }
                }
            }
            else if (unit is Monster monster)
            {
                UnitType unitType = _gameData.Get<UnitTypeSettings>(_gs.ch).Get(unit.UnitTypeId);

                List<UnitEffect> statEffects = unitType.Effects.Where(x => x.EntityTypeId == EntityTypes.Stat).ToList();

                List<UnitEffect> statPctEffects = unitType.Effects.Where(x=>x.EntityTypeId == EntityTypes.StatPct).ToList();    

                foreach (UnitKeyword unitKeyword in monster.ExtraKeywords)
                {
                    statEffects.AddRange(unitKeyword.Effects.Where(x=>x.EntityTypeId == EntityTypes.Stat));
                    statPctEffects.AddRange(unitKeyword.Effects.Where(x => x.EntityTypeId == EntityTypes.StatPct));
                }

                foreach (UnitEffect statEffect in statEffects)
                {
                    _statService.Set(unit, statEffect.EntityId, StatCategories.Bonus, statEffect.Quantity);
                }

                foreach (UnitEffect pctEffect in statPctEffects)
                {
                    _statService.Set(unit, pctEffect.EntityId, StatCategories.Pct, pctEffect.Quantity);
                }

                foreach (StatType statType in allStats)
                {
                    if (statType.IdKey >= StatConstants.PrimaryStatStart && statType.IdKey <= StatConstants.PrimaryStatEnd)
                    {
                        _statService.Set(unit, statType.IdKey, StatCategories.Base, unit.Level + statSettings.StartStat);
                    }
                    else if (buffStatTypes.Contains(statType.IdKey))
                    {
                        _statService.Set(unit, statType.IdKey, StatCategories.Base, statSettings.BaseBuffStatValue + unit.Level);
                    }

                }

                long minHealth = (long)(monsterSettings.BaseMinHealth + unit.Level * monsterSettings.MinHealthPerLevel);
                long maxHealth = (long)(monsterSettings.BaseMaxHealth + unit.Level * monsterSettings.MaxHealthPerLevel);

                monster.MinDam = (long)(monsterSettings.BaseMinDam + unit.Level * monsterSettings.MinDamPerLevel);
                monster.MaxDam = (long)(monsterSettings.BaseMaxDam + unit.Level * monsterSettings.MaxDamPerLevel);

                double healthScale = 1.0f;
                double damageScale = 1.0f;

                if (unit.FactionTypeId == FactionTypes.Player)
                {
                    double qualityPercent = _upgradeService.GetPartyBonus(party, PartyUpgrades.SummonQuality);

                    healthScale = (1 + qualityPercent/100.0f);
                    damageScale = (1 + qualityPercent/100.0f);
                }
                else
                {
                    healthScale *= (1 + monsterSettings.ExtraHealthScalePerLevel * unit.Level);
                    damageScale *= (1 + monsterSettings.ExtraDamageScalePerLevel * unit.Level);
                    healthScale *= (1 + party.DaysPlayed * combatSettings.MonsterExtraHealthScalePerDay);
                    damageScale *= (1 + party.DaysPlayed * combatSettings.MonsterExtraDamageScalePerDay);
                }

                minHealth = (long)(minHealth * healthScale);
                maxHealth = (long)(maxHealth * healthScale);
                monster.MinDam = (long)(monster.MinDam * damageScale);
                monster.MaxDam = (long)(monster.MaxDam * damageScale);


                long startHealth = 0;


                // Narrow health randomness a bit at higher levels.
                int healthCalcTimes = 2 + monster.Level / 10;

                for (int t = 0; t < healthCalcTimes; t++)
                {

                   startHealth += MathUtils.LongRange(minHealth, maxHealth, _rand);
                }

                startHealth /= healthCalcTimes;

                _statService.Set(unit, StatTypes.Health, StatCategories.Base, startHealth);
                _statService.Set(unit, StatTypes.Health, StatCategories.Curr, startHealth);

                long maxMana = unit.Level * monsterSettings.ManaPerLevel;

                _statService.Set(unit, StatTypes.Mana, StatCategories.Base, maxMana);
                _statService.Set(unit, StatTypes.Mana, StatCategories.Curr, maxMana);

            }
        }

        public long GetStatBonus(PartyData party, CrawlerUnit unit, long statTypeId)
        {
            if (statTypeId < 1)
            {
                return 0;
            }
            long statBonus = 0;
            long statValue = unit.Stats.Max(statTypeId);

            if (statValue >= 16)
            {
                statBonus = (long)Math.Ceiling(Math.Pow(statValue - 15, 2.0 / 3.0));
            }

            List<Role> roles = _gameData.Get<RoleSettings>(_gs.ch).GetRoles(unit.Roles);

            foreach (Role role in roles)
            {
                RoleBonusAmount amt = role.AmountBonuses.FirstOrDefault(x=>x.EntityTypeId == EntityTypes.StatBonus && x.EntityId == statTypeId);

                if (amt != null)
                {
                    statBonus += (int)amt.Amount;
                }
            }

            statBonus += (long)_upgradeService.GetPartyBonus(party, PartyUpgrades.StatBonusIncrease);

            statBonus += (long)_upgradeService.GetUnitBonus(unit, EntityTypes.StatBonus, statTypeId);

            if (unit.StatusEffects.HasBit(StatusEffects.Withered))
            {
                statBonus = statBonus * (100 - _gameData.Get<StatusEffectSettings>(_gs.ch).Get(StatusEffects.Withered).Amount) / 100;
            }

            return statBonus;

        }

        public void Add(PartyData party, CrawlerUnit unit, long statTypeId, int statCategory, long value, long elementTypeId = 0)
        {
            _statService.Add(unit, statTypeId, statCategory, value);
            _dispatcher.Dispatch(new RefreshUnitStatus() { Unit = unit, ElementTypeId = elementTypeId });
        }
    }
}
