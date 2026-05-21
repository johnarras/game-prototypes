using OxDb.SharedCore.Effects.Entities;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Inventory.Entities;
using OxDb.SharedGame.Inventory.PlayerData;

namespace OxDb.SharedGame.Crawler.Loot.Helpers
{
    public class RiddleLootTypeHelper : BaseCrawlerLootTypeHelper
    {
        public override long HelperKey => EntityTypes.Riddle;

        public override void AddEnchantToItem(PartyData party, Item item, ItemGenArgs args)
        {
            item.Effects.Add(new Effect()
            {
                EntityTypeId = EntityTypes.Riddle,
                EntityId = 1,
                Quantity = 1,
            });
        }
    }
}


