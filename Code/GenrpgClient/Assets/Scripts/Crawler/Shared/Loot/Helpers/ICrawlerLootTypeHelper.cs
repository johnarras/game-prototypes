using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Inventory.Entities;
using Genrpg.Shared.Inventory.PlayerData;

namespace Genrpg.Shared.Crawler.Loot.Helpers
{
    public interface ICrawlerLootTypeHelper : ISetupDictionaryItem<long>
    {
        void AddEnchantToItem(PartyData party, Item item, ItemGenArgs args);
    }
}
