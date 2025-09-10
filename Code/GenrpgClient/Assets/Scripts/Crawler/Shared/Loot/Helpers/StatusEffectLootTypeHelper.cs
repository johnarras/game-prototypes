using Genrpg.Shared.Crawler.Loot.Settings;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Inventory.Entities;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.UnitEffects.Constants;
using Genrpg.Shared.UnitEffects.Settings;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;

namespace Genrpg.Shared.Crawler.Loot.Helpers
{
    public class StatusEffectLootTypeHelper : BaseCrawlerLootTypeHelper
    {
        public override long Key => EntityTypes.StatusEffect;

        public override void AddEnchantToItem(PartyData party, Item item, ItemGenArgs args)
        {
            IReadOnlyList<StatusEffect> effects = _gameData.Get<StatusEffectSettings>(_gs.ch).GetData();

            CrawlerLootType lootType = _gameData.Get<CrawlerLootSettings>(_gs.ch).Get(Key);

            long maxRank = (int)(1 + item.Level * lootType.ScalingPerLevel);

            if (maxRank >= StatusEffects.Dead)
            {
                maxRank = StatusEffects.Dead - 1;
            }

            long rank = Math.Min(MathUtils.LongRange(0, maxRank - 1, _rand),
                MathUtils.LongRange(0, maxRank - 1, _rand));

            item.Effects.Add(new ItemEffect()
            {
                EntityTypeId = EntityTypes.StatusEffect,
                EntityId = effects[(int)rank].IdKey,
                Quantity = 1,
            });

        }
    }
}
