using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Inventory.Entities;
using OxDb.SharedGame.Inventory.PlayerData;

namespace OxDb.SharedGame.Crawler.Loot.Helpers
{
    public interface ICrawlerLootTypeHelper : ISetupDictionaryItem<long>
    {
        void AddEnchantToItem(PartyData party, Item item, ItemGenArgs args);
    }
}


