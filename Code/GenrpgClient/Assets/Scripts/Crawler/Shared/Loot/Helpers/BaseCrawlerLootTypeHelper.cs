using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Inventory.Entities;
using Genrpg.Shared.Inventory.PlayerData;

namespace Genrpg.Shared.Crawler.Loot.Helpers
{
    public abstract class BaseCrawlerLootTypeHelper : ICrawlerLootTypeHelper
    {
        protected IGameData _gameData = null;
        protected IClientGameState _gs = null;
        protected IClientRandom _rand = null;

        public abstract long Key { get; }
        public abstract void AddEnchantToItem(PartyData party, Item item, ItemGenArgs args);
    }
}
