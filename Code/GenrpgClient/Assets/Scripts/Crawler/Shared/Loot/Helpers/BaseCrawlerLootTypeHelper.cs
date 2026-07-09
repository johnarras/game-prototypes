using OxDb.SharedCore.GameSettings;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Inventory.Entities;
using OxDb.SharedGame.Inventory.PlayerData;

namespace OxDb.SharedGame.Crawler.Loot.Helpers
{
    public abstract class BaseCrawlerLootTypeHelper : ICrawlerLootTypeHelper
    {
        protected IGameData _gameData = null;
        protected IClientGameState _gs = null;

        public abstract long HelperKey { get; }
        public abstract void AddEnchantToItem(PartyData party, Item item, ItemGenArgs args);
    }
}


