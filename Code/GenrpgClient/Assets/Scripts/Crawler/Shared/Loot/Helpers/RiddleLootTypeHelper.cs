using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Inventory.Entities;
using Genrpg.Shared.Inventory.PlayerData;

namespace Genrpg.Shared.Crawler.Loot.Helpers
{
    public class RiddleLootTypeHelper : BaseCrawlerLootTypeHelper
    {
        public override long Key => EntityTypes.Riddle;

        public override void AddEnchantToItem(PartyData party, Item item, ItemGenArgs args)
        {
            item.Effects.Add(new ItemEffect()
            {
                EntityTypeId = EntityTypes.Riddle,
                EntityId = 1,
                Quantity = 1,
            });
        }
    }
}
