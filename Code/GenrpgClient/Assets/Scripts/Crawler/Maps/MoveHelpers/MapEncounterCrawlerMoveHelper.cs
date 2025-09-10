using Assets.Scripts.Crawler.ClientEvents.StatusPanelEvents;
using Assets.Scripts.Crawler.Maps.Services.Entities;
using Genrpg.Shared.Client.GameEvents;
using Genrpg.Shared.Crawler.Buffs.Constants;
using Genrpg.Shared.Crawler.Combat.Entities;
using Genrpg.Shared.Crawler.Loot.Services;
using Genrpg.Shared.Crawler.Maps.Constants;
using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Maps.Settings;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Spells.Constants;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.UnitEffects.Constants;
using Genrpg.Shared.UnitEffects.Settings;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Utils.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts.Crawler.Maps.MoveHelpers
{
    public class MapEncounterCrawlerMoveHelper : BaseCrawlerMoveHelper
    {
        public override int Order => 400;

        private ILootGenService _lootGenService = null;

        public override async Awaitable Execute(PartyData party, CrawlerMoveStatus moveStatus, CancellationToken token)
        {
            if (moveStatus.MoveIsComplete || !moveStatus.MovedPosition)
            {
                return;
            }

            CrawlerMap map = moveStatus.MapRoot.Map;

            long encounterTypeId = _mapService.GetEncounterAtCell(party, map, party.CurrPos.X, party.CurrPos.Z);

            CrawlerMapStatus mapStatus = party.GetMapStatus(map.IdKey, true);

            if (encounterTypeId == MapEncounters.Treasure)
            {

                LootGenData lootGenData = await _lootGenService.CreateLootGenData(party,
                    MathUtils.FloatRange(2.0f, 4.0f, _rand), MathUtils.FloatRange(2.0f, 4.0f, _rand), MathUtils.FloatRange(2.0f, 4.0f, _rand), "You Found a Great Treasure!", ECrawlerStates.ExploreWorld, null);

                mapStatus.OneTimeEncounters.Add(new PointXZ() { X = party.CurrPos.X, Z = party.CurrPos.Z });
                _mapService.ClearCellObject(party.CurrPos.X, party.CurrPos.Z);
                _crawlerService.ChangeState(ECrawlerStates.GiveLoot, token, lootGenData);
                moveStatus.MoveIsComplete = true;
            }
            else if (encounterTypeId == MapEncounters.Stats)
            {
                _crawlerService.ChangeState(ECrawlerStates.GainStats, token);
                moveStatus.MoveIsComplete = true;
            }
            else if (encounterTypeId == MapEncounters.Monsters)
            {
                InitialCombatState initialCombatState = new InitialCombatState()
                {
                    Difficulty = 1.5f,
                };
                _crawlerService.ChangeState(ECrawlerStates.StartCombat, token, initialCombatState);
                moveStatus.MoveIsComplete = true;
                return;
            }
            else if (encounterTypeId == MapEncounters.Trap)
            {

                if (party.Buffs.Get(PartyBuffs.Levitate) == 0 && !party.CurrentMap.Cleansed.HasBit(map.GetIndex(party.CurrPos.X, party.CurrPos.Z)))
                {
                    _dispatcher.Dispatch(new ShowFloatingText("It's a Trap!", EFloatingTextArt.Error));
                    CrawlerMapSettings mapSettings = _gameData.Get<CrawlerMapSettings>(_gs.ch);

                    IReadOnlyList<StatusEffect> effects = _gameData.Get<StatusEffectSettings>(_gs.ch).GetData();

                    long maxStatusEffectTier = Math.Min(StatusEffects.Dead - 1, (int)(map.Level * mapSettings.TrapDebuffLevelScaling));

                    int minDam = map.Level * mapSettings.TrapMinDamPerLevel;
                    int maxDam = map.Level * mapSettings.TrapMaxDamagePerLevel;

                    foreach (PartyMember pm in party.GetActiveParty())
                    {
                        double luckBonus = _crawlerStatService.GetStatBonus(party, pm, StatTypes.Luck) / 100.0f;

                        if (_rand.NextDouble() < mapSettings.TrapHitChance - luckBonus)
                        {
                            continue;
                        }

                        int damage = MathUtils.IntRange(minDam, maxDam, _rand);
                        _crawlerStatService.Add(party, pm, StatTypes.Health, StatCategories.Curr, -damage, ElementTypes.Melee);

                        if (pm.Stats.Curr(StatTypes.Health) < 1)
                        {
                            pm.StatusEffects.SetBit(StatusEffects.Dead);
                            continue;
                        }

                        if (_rand.NextDouble() < mapSettings.TrapDebuffChance && maxStatusEffectTier > 0)
                        {
                            long tier = Math.Min(MathUtils.LongRange(1, maxStatusEffectTier, _rand), MathUtils.LongRange(1, maxStatusEffectTier, _rand));


                            StatusEffect effect = effects.FirstOrDefault(x => x.IdKey == tier);

                            if (effect != null)
                            {
                                pm.StatusEffects.SetBit(tier);
                            }
                        }
                    }

                    if (await _partyService.CheckIfPartyIsDead(party, token))
                    {
                        moveStatus.MoveIsComplete = true;
                    }
                    _dispatcher.Dispatch(new RefreshPartyStatus());
                }
                _mapService.ClearCellObject(party.CurrPos.X, party.CurrPos.Z);
            }
        }
    }
}
