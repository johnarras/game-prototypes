using Genrpg.Shared.Crawler.Loot.Settings;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Inventory.Entities;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Utils;
using System.Collections.Generic;
using System.Linq;

namespace Genrpg.Shared.Crawler.Loot.Helpers
{
    public class CrawlerSpellLootTypeHelper : BaseCrawlerLootTypeHelper
    {
        public override long HelperKey => EntityTypes.CrawlerSpell;

        public override void AddEnchantToItem(PartyData party, Item item, ItemGenArgs args)
        {
            CrawlerLootType lootType = _gameData.Get<CrawlerLootSettings>(_gs.ch).Get(HelperKey);

            long effectLevel = (long)(1 + (args.Level * lootType.ScalingPerLevel));

            List<CrawlerSpell> okSpells = _gameData.Get<CrawlerSpellSettings>(_gs.ch).GetData().Where(x=>x.RoleScalingTier <= effectLevel).ToList();

            CrawlerSpell spell = RandomUtils.GetRandomEnchant(okSpells, _rand);

            if (spell != null && lootType != null)
            {
                item.Effects.Add(new ItemEffect()
                {
                    EntityTypeId = EntityTypes.CrawlerSpell,
                    EntityId = spell.IdKey,
                    Quantity = effectLevel,
                });
            }
        }
    }
}


