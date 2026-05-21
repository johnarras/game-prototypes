using Assets.Scripts.Crawler.Maps.Services.Entities;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Crawler.Maps.Constants;
using OxDb.SharedGame.Crawler.Maps.Settings;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Spells.Constants;
using OxDb.SharedGame.Stats.Constants;
using OxDb.SharedGame.UnitEffects.Constants;
using System;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts.Crawler.Maps.MoveHelpers
{
    public class ApplyMagicCrawlerMoveHelper : BaseCrawlerMoveHelper
    {
        public override int Order => 800;

        public override async Awaitable Execute(PartyData party, CrawlerMoveStatus status, CancellationToken token)
        {
            if (_mapService.HasMagicBit(party.CurrPos.X, party.CurrPos.Z, MapMagics.Spinner, true))
            {
                int rotateAmount = RandUtils.IntRange(-1, 2, _rand.Rand);
                if (rotateAmount != 0)
                {
                    await _moveService.Rot(status, rotateAmount, true, token);
                }
            }
            if (_mapService.HasMagicBit(party.CurrPos.X, party.CurrPos.Z, MapMagics.NoMagic, true))
            {
                party.Buffs.Clear();
            }
            if (_mapService.HasMagicBit(party.CurrPos.X, party.CurrPos.Z, MapMagics.Drain, true))
            {
                CrawlerMapSettings mapSettings = _gameData.Get<CrawlerMapSettings>(_gs.ch);

                foreach (PartyMember pm in party.ActiveParty)
                {
                    if (pm.StatusEffects.HasBitIndex(StatusEffects.Dead))
                    {
                        continue;
                    }

                    int healthLost = Math.Max(1, (int)(pm.Stats.Max(StatTypes.Health) * mapSettings.DrainHealthPercent));
                    healthLost = Math.Min(healthLost, pm.Stats.Curr(StatTypes.Health));
                    _crawlerStatService.Add(party, pm, StatTypes.Health, UnitStatValOffsets.Curr, -healthLost, ElementTypes.Melee);
                    if (pm.Stats.Curr(StatTypes.Health) < 1)
                    {
                        pm.StatusEffects.SetBitIndex(StatusEffects.Dead);
                        continue;
                    }

                    int manaLost = Math.Max(1, (int)(pm.Stats.Max(StatTypes.Mana) * mapSettings.DrainManaPercent));
                    manaLost = Math.Min(manaLost, pm.Stats.Curr(StatTypes.Mana));
                    _crawlerStatService.Add(party, pm, StatTypes.Mana, UnitStatValOffsets.Curr, -manaLost);
                }

                if (await _partyService.CheckIfPartyIsDead(party, token))
                {
                    status.MoveIsComplete = true;
                }
            }
        }
    }
}


