using Assets.Scripts.Audio.ClientEvents;
using Assets.Scripts.Crawler.ClientEvents.StatusPanelEvents;
using Assets.Scripts.Crawler.Constants;
using Assets.Scripts.Crawler.Maps.GameObjects;
using Assets.Scripts.Crawler.Maps.Loading;
using Assets.Scripts.Crawler.Maps.Services.Entities;
using Assets.Scripts.FloatingText.ClientEvents;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Crawler.Buffs.Constants;
using OxDb.SharedGame.Crawler.Maps.Constants;
using OxDb.SharedGame.Crawler.Maps.Entities;
using OxDb.SharedGame.Crawler.Maps.Settings;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.Party.Services;
using OxDb.SharedGame.Crawler.Stats.Services;
using OxDb.SharedGame.Crawler.Worlds.Entities;
using OxDb.SharedGame.Spells.Constants;
using OxDb.SharedGame.Stats.Constants;
using OxDb.SharedGame.UnitEffects.Constants;
using OxDb.SharedGame.UnitEffects.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.Crawler.Maps.EncounterHelpers
{
    public class TrapMapEncounterHelper : BaseClientMapEncounterHelper
    {
        private ICrawlerStatService _crawlerStatService = null;
        private IDispatcher _dispatcher = null;
        private IPartyService _partyService = null;

        public override long HelperKey => MapEncounters.Trap;

        public override async ValueTask DrawCell(PartyData party, CrawlerWorld world, CrawlerMapRoot mapRoot, ClientMapCell cell, int x, int z, CancellationToken token)
        {

            CrawlerObjectLoadData loadData = new CrawlerObjectLoadData()
            {
                MapRoot = mapRoot,
                Cell = cell,
            };

            _mapService.LoadProp(loadData, "Trap", token);

            await Task.CompletedTask;
        }

        public override async ValueTask OnEnterCell(PartyData party, CrawlerMap map, CrawlerMapStatus mapStatus, CrawlerMoveStatus moveStatus, CancellationToken token)
        {
            if (party.Buffs[PartyBuffs.Levitate] == 0 && !party.CurrentMap.Cleansed.HasBitIndex(map.GetIndex(party.CurrPos.X, party.CurrPos.Z)))
            {
                _dispatcher.Dispatch(new ShowFloatingText("It's a Trap!", EFloatingTextArt.Error));
                CrawlerMapSettings mapSettings = _gameData.Get<CrawlerMapSettings>(_gs.ch);

                IReadOnlyList<StatusEffect> effects = _gameData.Get<StatusEffectSettings>(_gs.ch).GetData();

                long maxStatusEffectTier = Math.Min(StatusEffects.Dead - 1, (int)(map.Level * mapSettings.TrapDebuffLevelScaling));

                long minDam = map.Level * mapSettings.TrapMinDamPerLevel;
                long maxDam = map.Level * mapSettings.TrapMaxDamagePerLevel;

                _dispatcher.Dispatch(new PlaySound(CrawlerAudio.TrapClose));
                foreach (PartyMember pm in party.ActiveParty)
                {
                    double luckBonus = _crawlerStatService.GetStatBonus(party, pm, StatTypes.Luck) / 100.0f;

                    if (_gs.Rand.NextDouble() < mapSettings.TrapHitChance - luckBonus)
                    {
                        continue;
                    }

                    long damage = RandUtils.LongRange(minDam, maxDam, _gs.Rand);
                    _crawlerStatService.Add(party, pm, StatTypes.Health, UnitStatValOffsets.Curr, -damage, ElementTypes.Melee);

                    if (pm.Stats.Curr(StatTypes.Health) < 1)
                    {
                        pm.StatusEffects.SetBitIndex(StatusEffects.Dead);
                        continue;
                    }

                    if (_gs.Rand.NextDouble() < mapSettings.TrapDebuffChance && maxStatusEffectTier > 0)
                    {
                        long tier = Math.Min(RandUtils.LongRange(1, maxStatusEffectTier, _gs.Rand), RandUtils.LongRange(1, maxStatusEffectTier, _gs.Rand));


                        StatusEffect effect = effects.FirstOrDefault(x => x.IdKey == tier);

                        if (effect != null)
                        {
                            pm.StatusEffects.SetBitIndex(tier);
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


