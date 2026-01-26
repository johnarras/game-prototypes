using Assets.Scripts.Crawler.ClientEvents.StatusPanelEvents;
using Assets.Scripts.Crawler.Maps.GameObjects;
using Assets.Scripts.Crawler.Maps.Loading;
using Assets.Scripts.Crawler.Maps.Services.Entities;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Client.GameEvents;
using Genrpg.Shared.Crawler.Buffs.Constants;
using Genrpg.Shared.Crawler.Maps.Constants;
using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Maps.Settings;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Party.Services;
using Genrpg.Shared.Crawler.Stats.Services;
using Genrpg.Shared.Crawler.Worlds.Entities;
using Genrpg.Shared.Spells.Constants;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.UnitEffects.Constants;
using Genrpg.Shared.UnitEffects.Settings;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Crawler.Maps.EncounterHelpers
{
    public class TrapMapEncounterHelper : BaseClientMapEncounterHelper
    {
        private ICrawlerStatService _crawlerStatService = null;
        private IDispatcher _dispatcher = null;
        private IPartyService _partyService = null;

        public override long HelperKey => MapEncounters.Trap;

        public override async Awaitable DrawCell(PartyData party, CrawlerWorld world, CrawlerMapRoot mapRoot, ClientMapCell cell, int x, int z, CancellationToken token)
        {
            LoadPropAtCell(mapRoot, cell, "Trap", x, z, null, token);

            await Task.CompletedTask;
        }

        public override async Awaitable OnEnterCell(PartyData party, CrawlerMap map, CrawlerMapStatus mapStatus, CrawlerMoveStatus moveStatus, CancellationToken token)
        {
            if (party.Buffs[PartyBuffs.Levitate] == 0 && !party.CurrentMap.Cleansed.HasBit(map.GetIndex(party.CurrPos.X, party.CurrPos.Z)))
            {
                _dispatcher.Dispatch(new ShowFloatingText("It's a Trap!", EFloatingTextArt.Error));
                CrawlerMapSettings mapSettings = _gameData.Get<CrawlerMapSettings>(_gs.ch);

                IReadOnlyList<StatusEffect> effects = _gameData.Get<StatusEffectSettings>(_gs.ch).GetData();

                long maxStatusEffectTier = Math.Min(StatusEffects.Dead - 1, (int)(map.Level * mapSettings.TrapDebuffLevelScaling));

                long minDam = map.Level * mapSettings.TrapMinDamPerLevel;
                long maxDam = map.Level * mapSettings.TrapMaxDamagePerLevel;

                foreach (PartyMember pm in party.ActiveParty)
                {
                    double luckBonus = _crawlerStatService.GetStatBonus(party, pm, StatTypes.Luck) / 100.0f;

                    if (_rand.NextDouble() < mapSettings.TrapHitChance - luckBonus)
                    {
                        continue;
                    }

                    long damage = MathUtil.LongRange(minDam, maxDam, _rand);
                    _crawlerStatService.Add(party, pm, StatTypes.Health, StatCategories.Curr, -damage, ElementTypes.Melee);

                    if (pm.Stats.Curr(StatTypes.Health) < 1)
                    {
                        pm.StatusEffects.SetBit(StatusEffects.Dead);
                        continue;
                    }

                    if (_rand.NextDouble() < mapSettings.TrapDebuffChance && maxStatusEffectTier > 0)
                    {
                        long tier = Math.Min(MathUtil.LongRange(1, maxStatusEffectTier, _rand), MathUtil.LongRange(1, maxStatusEffectTier, _rand));


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

        protected override void AfterDownloadProp(GameObject prop, CrawlerObjectLoadData args)
        {
        }
    }
}


