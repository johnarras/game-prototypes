using Assets.Scripts.Crawler.ClientEvents.StatusPanelEvents;
using OxDb.SharedCore.Effects.Entities;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Crawler.Combat.Settings;
using OxDb.SharedGame.Crawler.Crawlers.Services;
using OxDb.SharedGame.Crawler.Monsters.Entities;
using OxDb.SharedGame.Crawler.Monsters.Settings;
using OxDb.SharedGame.Crawler.Options.Constants;
using OxDb.SharedGame.Crawler.Options.Services;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.Party.Services;
using OxDb.SharedGame.Crawler.Roles.Settings;
using OxDb.SharedGame.Crawler.Spells.Settings;
using OxDb.SharedGame.Crawler.Stats.Settings;
using OxDb.SharedGame.Crawler.Training.Settings;
using OxDb.SharedGame.Crawler.Upgrades.Constants;
using OxDb.SharedGame.Factions.Constants;
using OxDb.SharedGame.Inventory.PlayerData;
using OxDb.SharedGame.Stats.Constants;
using OxDb.SharedGame.Stats.Entities;
using OxDb.SharedGame.Stats.Settings.Stats;
using OxDb.SharedGame.UnitEffects.Constants;
using OxDb.SharedGame.UnitEffects.Settings;
using OxDb.SharedGame.Units.Entities;
using OxDb.SharedGame.Units.Settings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OxDb.SharedGame.Crawler.Stats.Services
{
    public interface ICrawlerStatService : IInjectable
    {
        void CalcUnitStats(PartyData party, CrawlerUnit unit, bool resetCurrStats);

        void CalcPartyStats(PartyData party, bool resetCurrStats);

        long GetStatBonus(PartyData party, CrawlerUnit unit, long statId);

        void Add(PartyData party, CrawlerUnit unit, long statTypeId, int statCategory, long value, long elementTypeId = 0);

        void FullyRestParty(PartyData party);

    }

    public class CrawlerStatService : ICrawlerStatService
    {
        protected IStatService _statService = null;
        protected IGameData _gameData = null;
        protected IClientGameState _gs = null;
        private ICrawlerUpgradeService _upgradeService = null;
        private IDispatcher _dispatcher = null;
        protected IPartyService _partyService = null;
        private ICrawlerOptionsService _optionsService = null;

        public void CalcPartyStats(PartyData party, bool resetCurrStats)
        {
            foreach (PartyMember member in party.ActiveParty)
            {
                CalcUnitStats(party, member, resetCurrStats);
            }
            _partyService.UpdateItemBuffs(party);
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
            CrawlerSpellSettings spellSettings = _gameData.Get<CrawlerSpellSettings>(_gs.ch);

            IReadOnlyList<StatType> allStats = _gameData.Get<StatSettings>(_gs.ch).GetData();

            IReadOnlyList<Role> allRoles = roleSettings.GetData();

            List<long> buffStatTypes = new List<long> { StatTypes.Armor, StatTypes.Resist, StatTypes.Speed, StatTypes.Hit };

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
                    _statService.Add(member, primaryStatId, UnitStatValOffsets.Base, member.GetPermStat(primaryStatId));
                    _statService.Add(member, primaryStatId, UnitStatValOffsets.Pct, bonusPercent);
                }

                foreach (long buffStatType in buffStatTypes)
                {
                    _statService.Set(member, buffStatType, UnitStatValOffsets.Base, statSettings.BaseBuffStatValue + member.Level);

                }

                // Now do equipment.

                foreach (Item item in member.Equipment)
                {
                    foreach (Effect eff in item.Effects)
                    {
                        if (eff.EntityTypeId == EntityTypes.Stat)
                        {
                            _statService.Add(member, eff.EntityId, UnitStatValOffsets.Bonus, eff.Quantity);
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

                _statService.Set(member, StatTypes.Health, UnitStatValOffsets.Base, totalHealth);
                _statService.Set(member, StatTypes.Mana, UnitStatValOffsets.Base, totalMana);

                foreach (long mutableStatType in mutableStatTypes)
                {
                    long currStatVal = currStats.FirstOrDefault(x => x.StatTypeId == mutableStatType).Val;
                    long maxStatVal = member.Stats.Max(mutableStatType);

                    if (resetCurrStats || currStatVal > maxStatVal)
                    {
                        _statService.Set(member, mutableStatType, UnitStatValOffsets.Curr, maxStatVal);
                    }
                    else
                    {
                        _statService.Set(member, mutableStatType, UnitStatValOffsets.Curr, currStatVal);
                    }
                }

                // Now give bonus stats.
                foreach (StatType stype in allStats)
                {
                    if (stype.BonusStatTypeId > 0)
                    {
                        _statService.Add(unit, stype.BonusStatTypeId, UnitStatValOffsets.Pct, GetStatBonus(party, member, stype.IdKey));
                    }
                }
            }
            else if (unit is Monster monster)
            {
                UnitType unitType = _gameData.Get<UnitTypeSettings>(_gs.ch).Get(unit.UnitTypeId);

                List<Effect> statEffects = unitType.Effects.Where(x => x.EntityTypeId == EntityTypes.Stat).ToList();

                List<Effect> statPctEffects = unitType.Effects.Where(x => x.EntityTypeId == EntityTypes.StatPct).ToList();

                foreach (UnitKeyword unitKeyword in monster.ExtraKeywords)
                {
                    statEffects.AddRange(unitKeyword.Effects.Where(x => x.EntityTypeId == EntityTypes.Stat));
                    statPctEffects.AddRange(unitKeyword.Effects.Where(x => x.EntityTypeId == EntityTypes.StatPct));
                }

                foreach (Effect statEffect in statEffects)
                {
                    _statService.Set(unit, statEffect.EntityId, UnitStatValOffsets.Bonus, statEffect.Quantity);
                }

                foreach (Effect pctEffect in statPctEffects)
                {
                    _statService.Set(unit, pctEffect.EntityId, UnitStatValOffsets.Pct, pctEffect.Quantity);
                }

                foreach (StatType statType in allStats)
                {
                    if (statType.IdKey >= StatConstants.PrimaryStatStart && statType.IdKey <= StatConstants.PrimaryStatEnd)
                    {
                        _statService.Set(unit, statType.IdKey, UnitStatValOffsets.Base, (long)(unit.Level * monsterSettings.PrimaryStatsPointsPerLevel) + statSettings.MinStartValue);
                    }
                    else if (buffStatTypes.Contains(statType.IdKey))
                    {
                        _statService.Set(unit, statType.IdKey, UnitStatValOffsets.Base, statSettings.BaseBuffStatValue + unit.Level);
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
                    if (monster.SummonArgs != null)
                    {
                        minHealth = (minHealth + maxHealth) / 2;
                        maxHealth = minHealth;
                    }

                    double qualityPercent = _upgradeService.GetPartyBonus(party, PartyUpgrades.SummonQuality);

                    healthScale = (1 + qualityPercent / 100.0f);
                    damageScale = (1 + qualityPercent / 100.0f);

                    if (!_optionsService.HasOption(party, CrawlerOptions.WholeParty))
                    {
                        healthScale *= 1.5f;
                        damageScale *= 1.5f;
                    }
                }
                else
                {
                    healthScale *= (1 + monsterSettings.ExtraHealthScalePerLevel * unit.Level);
                    damageScale *= (1 + monsterSettings.ExtraDamageScalePerLevel * unit.Level);


                    if (_optionsService.HasOption(party, CrawlerOptions.MonstersGetStronger))
                    {
                        healthScale *= (1 + party.DaysPlayed * combatSettings.MonsterExtraHealthScalePerDay);
                        damageScale *= (1 + party.DaysPlayed * combatSettings.MonsterExtraDamageScalePerDay);
                    }
                    if (_optionsService.HasOption(party, CrawlerOptions.HarderMonsters))
                    {
                        healthScale *= 1.25f;
                        damageScale *= 1.25f;
                    }
                }

                minHealth = (long)(minHealth * healthScale);
                maxHealth = (long)(maxHealth * healthScale);
                monster.MinDam = (long)(monster.MinDam * damageScale);
                monster.MaxDam = (long)(monster.MaxDam * damageScale);


                long startHealth = 0;


                // Narrow health randomness a bit at higher levels.
                long healthCalcTimes = 2 + monster.Level / 10;

                for (int t = 0; t < healthCalcTimes; t++)
                {

                    startHealth += RandUtils.LongRange(minHealth, maxHealth, _gs.Rand);
                }

                startHealth /= healthCalcTimes;

                _statService.Set(unit, StatTypes.Health, UnitStatValOffsets.Base, startHealth);
                _statService.Set(unit, StatTypes.Health, UnitStatValOffsets.Curr, startHealth);

                long maxMana = unit.Level * monsterSettings.ManaPerLevel;

                _statService.Set(unit, StatTypes.Mana, UnitStatValOffsets.Base, maxMana);
                _statService.Set(unit, StatTypes.Mana, UnitStatValOffsets.Curr, maxMana);

            }
        }

        public long GetBaseStatBonus(long statValue)
        {
            CrawlerStatSettings settings = _gameData.Get<CrawlerStatSettings>(_gs.ch);

            double bonusValue = settings.BonusScalingMult * Math.Pow(statValue - settings.BonusScalingStartVal, settings.BonusScalingPower);

            return (long)bonusValue;
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
                statBonus = GetBaseStatBonus(statValue);
            }

            if (unit is Monster monster)
            {
                if (monster.SummonArgs != null)
                {
                    statBonus += monster.SummonArgs.SummonStatBonus;
                }
                return statBonus;
            }

            List<Role> roles = _gameData.Get<RoleSettings>(_gs.ch).GetRoles(unit.Roles);

            foreach (Role role in roles)
            {
                RoleBonusAmount amt = role.AmountBonuses.FirstOrDefault(x => x.EntityTypeId == EntityTypes.StatBonus && x.EntityId == statTypeId);

                if (amt != null)
                {
                    statBonus += (int)amt.Amount;
                }
            }

            statBonus += (long)_upgradeService.GetPartyBonus(party, PartyUpgrades.StatBonusIncrease);

            statBonus += (long)_upgradeService.GetUnitBonus(unit, EntityTypes.StatBonus, statTypeId);

            if (unit.StatusEffects.HasBitIndex(StatusEffects.Withered))
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

        public void FullyRestParty(PartyData party)
        {
            foreach (PartyMember member in party.ActiveParty)
            {
                member.Stats.SetCurr(StatTypes.Health, member.Stats.Max(StatTypes.Health));
                member.Stats.SetCurr(StatTypes.Mana, member.Stats.Max(StatTypes.Mana));
                member.StatusEffects.Clear();
            }

            party.Buffs.Clear();
        }
    }
}


