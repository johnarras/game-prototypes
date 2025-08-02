using Assets.Scripts.Crawler.Maps.Services.Entities;
using Genrpg.Shared.Crawler.GameEvents;
using Genrpg.Shared.Crawler.Maps.Constants;
using Genrpg.Shared.Crawler.Maps.Settings;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Spells.Constants;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.UnitEffects.Constants;
using Genrpg.Shared.Utils;
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
            if (_mapService.HasMagicBit(party.CurrPos.X, party.CurrPos.Z, MapMagics.Spinner))
            {
                int rotateAmount = MathUtils.IntRange(-1, 2, _rand);
                if (rotateAmount != 0)
                {
                    await _moveService.Rot(status, rotateAmount, true, token);
                }
            }
            if (_mapService.HasMagicBit(party.CurrPos.X, party.CurrPos.Z, MapMagics.NoMagic))
            {
                party.Buffs.Clear();
            }
            if (_mapService.HasMagicBit(party.CurrPos.X, party.CurrPos.Z, MapMagics.Drain))
            {
                CrawlerMapSettings mapSettings = _gameData.Get<CrawlerMapSettings>(_gs.ch);

                foreach (PartyMember pm in party.GetActiveParty())
                {
                    if (pm.StatusEffects.HasBit(StatusEffects.Dead))
                    {
                        continue;
                    }

                    int healthLost = Math.Max(1, (int)(pm.Stats.Max(StatTypes.Health) * mapSettings.DrainHealthPercent));
                    healthLost = Math.Min(healthLost, pm.Stats.Curr(StatTypes.Health));
                    _crawlerStatService.Add(party, pm, StatTypes.Health, StatCategories.Curr, -healthLost, ElementTypes.Physical);
                    if (pm.Stats.Curr(StatTypes.Health) < 1)
                    {
                        pm.StatusEffects.SetBit(StatusEffects.Dead);
                        continue;
                    }

                    int manaLost = Math.Max(1, (int)(pm.Stats.Max(StatTypes.Mana) * mapSettings.DrainManaPercent));
                    manaLost = Math.Min(manaLost, pm.Stats.Curr(StatTypes.Mana));
                    _crawlerStatService.Add(party, pm, StatTypes.Mana, StatCategories.Curr, -manaLost);
                }

                if (await _partyService.CheckIfPartyIsDead(party, token))
                {
                    status.MoveIsComplete = true;
                }
            }
        }
    }
}
