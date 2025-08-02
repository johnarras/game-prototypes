using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Stats.Services;
using Genrpg.Shared.Crawler.TimeOfDay.Constants;
using Genrpg.Shared.Crawler.TimeOfDay.Settings;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.Zones.Settings;
using System.Linq;
using System.Threading.Tasks;
using Genrpg.Shared.Crawler.GameEvents;
using Genrpg.Shared.Crawler.Maps.Services;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Crawler.Buffs.Constants;
using Assets.Scripts.Crawler.ClientEvents.StatusPanelEvents;
using Genrpg.Shared.Crawler.Worlds.Entities;
using Genrpg.Shared.Crawler.Loot.Services;
using System;
using Genrpg.Shared.UnitEffects.Constants;
using Genrpg.Shared.Client.GameEvents;

namespace Genrpg.Shared.Crawler.TimeOfDay.Services
{
    public interface ITimeOfDayService : IInjectable
    {
        Task UpdateTime(PartyData party, ECrawlerTimeUpdateTypes updateType);
    }
    public class TimeOfDayService : ITimeOfDayService
    {

        const int SecondsPerMinute = 60;
        const int MinutesPerHour = 60;
        const int HoursPerDay = 24;

        const int SecondsPerDay = SecondsPerMinute * MinutesPerHour * HoursPerDay;

        private IStatService _statService = null;
        private IGameData _gameData = null;
        private IClientGameState _gs = null;
        private ICrawlerWorldService _worldService = null;
        private IDispatcher _dispatcher = null;
        private ILootGenService _lootService = null;

        public async Task UpdateTime(PartyData party, ECrawlerTimeUpdateTypes type)
        {
            CrawlerWorld world = await _worldService.GetWorld(party.WorldId);
            TimeOfDaySettings timeSettings = _gameData.Get<TimeOfDaySettings>(_gs.ch);

            double hoursSpent = 0;

            bool fullHeal = false;

            CrawlerMap map = _worldService.GetMap(party.CurrPos.MapId);

            if (type == ECrawlerTimeUpdateTypes.Move)
            {

                long zoneTypeId = map.Get(party.CurrPos.X, party.CurrPos.Z, CellIndex.Terrain);

                long regionTypeId = map.Get(party.CurrPos.X, party.CurrPos.Z, CellIndex.Region);

                if (regionTypeId > 0)
                {
                    zoneTypeId = regionTypeId;
                }

                ZoneType zoneType = _gameData.Get<ZoneTypeSettings>(_gs.ch).Get(zoneTypeId);

                double traversalScale = 1;

                if (zoneType != null && zoneType.TraveralTimeScale > 0)
                {
                    traversalScale = zoneType.TraveralTimeScale;
                }

                if (timeSettings.LevitateSpeedup > 1 && party.Buffs.Get(PartyBuffs.Levitate) > 0)
                {
                    traversalScale /= (timeSettings.LevitateSpeedup * party.Buffs.Get(PartyBuffs.Levitate));
                }
                long overloadedInventoryCount = Math.Max(0, party.Inventory.Count - _lootService.GetPartyInventorySize(party));

                traversalScale *= (1 + overloadedInventoryCount * timeSettings.MoveSpeedIncreasePerExtraInventoryItem);

                hoursSpent = timeSettings.BaseMoveMinutes * (1.0 / MinutesPerHour) * traversalScale;

                if (overloadedInventoryCount > 0)
                {
                    _dispatcher.Dispatch(new ShowFloatingText("You are carrying too many items!", EFloatingTextArt.Error));
                }

            }
            else if (type == ECrawlerTimeUpdateTypes.CombatRound)
            {
                hoursSpent = timeSettings.CombatRoundMinutes / MinutesPerHour;
            }
            else if (type == ECrawlerTimeUpdateTypes.Rest)
            {
                hoursSpent = timeSettings.RestHours;
                fullHeal = true;
            }
            else if (type == ECrawlerTimeUpdateTypes.Eat)
            {
                hoursSpent = timeSettings.EatHours;
            }
            else if (type == ECrawlerTimeUpdateTypes.Drink)
            {
                hoursSpent = timeSettings.DrinkHours;
            }
            else if(type == ECrawlerTimeUpdateTypes.Rumor)
            {
                hoursSpent += timeSettings.RumorHours;  
            }
            else if (type == ECrawlerTimeUpdateTypes.Tavern)
            {
                hoursSpent = timeSettings.DailyResetHour - party.HourOfDay;
                if (hoursSpent < 0)
                {
                    hoursSpent += HoursPerDay;
                }
                fullHeal = true;
            }

            party.HourOfDay += (float)hoursSpent;

            while (party.HourOfDay > HoursPerDay)
            {
                party.HourOfDay -= HoursPerDay;
                party.DaysPlayed++;
            }

            foreach (PartyMember member in party.GetActiveParty())
            {
                if (member.StatusEffects.MatchAnyBits(-1))
                {
                    continue;
                }

                bool didAdjustStat = false;
                foreach (StatRegenHours hours in timeSettings.RegenHours)
                {
                    long maxVal = member.Stats.Max(hours.StatTypeId);
                    long currVal = member.Stats.Curr(hours.StatTypeId);

                    if (currVal >= maxVal)
                    {
                        continue;
                    }

                    StatRegenFraction fraction = member.RegenFractions.FirstOrDefault(x => x.StatTypeId == hours.StatTypeId);
                    if (fraction == null)
                    {
                        fraction = new StatRegenFraction()
                        {
                            StatTypeId = hours.StatTypeId,
                        };
                        member.RegenFractions.Add(fraction);
                    }

                    float regenPercent = (float)(hoursSpent / hours.RegenHours);

                    if (fullHeal)
                    {
                        regenPercent = 1;
                    }

                    fraction.Fraction += regenPercent * maxVal;

                    long currRegen = (long)fraction.Fraction;
                    fraction.Fraction -= currRegen;

                    if (hours.StatTypeId == StatTypes.Health && member.StatusEffects.HasBit(StatusEffects.Poisoned))
                    {
                        currRegen = -currRegen;
                    }
                    if (hours.StatTypeId == StatTypes.Mana && member.StatusEffects.HasBit(StatusEffects.Diseased))
                    {
                        currRegen = -currRegen;
                    }
                    if (currRegen > 0)
                    {
                        currVal += currRegen;
                        if (currVal > maxVal)
                        {
                            currVal = maxVal;
                        }
                        if (currVal < 0)
                        {
                            currVal = 0;
                        }

                        _statService.Set(member, fraction.StatTypeId, StatCategories.Curr, currVal);
                        didAdjustStat = true;
                        if (currVal >= maxVal)
                        {
                            member.RegenFractions.Remove(fraction);
                        }
                        if (fraction.StatTypeId == StatTypes.Health && currVal == 0)
                        {
                            member.StatusEffects.SetBit(StatusEffects.Dead);
                        }
                    }
                }

                if (didAdjustStat)
                {
                    _dispatcher.Dispatch(new RefreshUnitStatus() { Unit = member });
                }
            }

            _dispatcher.Dispatch(new UpdateCrawlerUI());
        }
    }
}
