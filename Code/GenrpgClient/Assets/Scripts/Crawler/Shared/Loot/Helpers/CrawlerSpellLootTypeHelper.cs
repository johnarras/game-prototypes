using Genrpg.Shared.Crawler.Loot.Settings;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Spells.Settings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Inventory.Entities;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Utils;

namespace Genrpg.Shared.Crawler.Loot.Helpers
{
    public class CrawlerSpellLootTypeHelper : BaseCrawlerLootTypeHelper
    {
        public override long Key => EntityTypes.CrawlerSpell;

        public override void AddEnchantToItem(PartyData party, Item item, ItemGenArgs args)
        {
            CrawlerSpell spell = RandomUtils.GetRandomEnchant(_gameData.Get<CrawlerSpellSettings>(_gs.ch).GetData(), _rand);

            CrawlerLootType lootType = _gameData.Get<CrawlerLootSettings>(_gs.ch).Get(Key);

            if (spell != null)
            {
                item.Effects.Add(new ItemEffect()
                {
                    EntityTypeId = EntityTypes.CrawlerSpell,
                    EntityId = spell.IdKey,
                    Quantity = (long)(1 + (args.Level * lootType.ScalingPerLevel)),
                });
            }
        }
    }
}
