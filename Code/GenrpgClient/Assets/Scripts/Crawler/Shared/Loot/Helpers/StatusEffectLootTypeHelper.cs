using OxDb.SharedCore.Effects.Entities;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Crawler.Loot.Settings;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Inventory.Entities;
using OxDb.SharedGame.Inventory.PlayerData;
using OxDb.SharedGame.UnitEffects.Constants;
using OxDb.SharedGame.UnitEffects.Settings;
using System;
using System.Collections.Generic;

namespace OxDb.SharedGame.Crawler.Loot.Helpers
{
    public class StatusEffectLootTypeHelper : BaseCrawlerLootTypeHelper
    {
        public override long HelperKey => EntityTypes.StatusEffect;

        public override void AddEnchantToItem(PartyData party, Item item, ItemGenArgs args)
        {
            IReadOnlyList<StatusEffect> effects = _gameData.Get<StatusEffectSettings>(_gs.ch).GetData();

            CrawlerLootType lootType = _gameData.Get<CrawlerLootSettings>(_gs.ch).Get(HelperKey);

            long maxRank = (int)(1 + item.Level * lootType.ScalingPerLevel);

            if (maxRank >= StatusEffects.Dead)
            {
                maxRank = StatusEffects.Dead - 1;
            }

            long rank = Math.Min(RandUtils.LongRange(0, maxRank - 1, _gs.Rand),
                RandUtils.LongRange(0, maxRank - 1, _gs.Rand));

            item.Effects.Add(new Effect()
            {
                EntityTypeId = EntityTypes.StatusEffect,
                EntityId = effects[(int)rank].IdKey,
                Quantity = 1,
            });

        }
    }
}


