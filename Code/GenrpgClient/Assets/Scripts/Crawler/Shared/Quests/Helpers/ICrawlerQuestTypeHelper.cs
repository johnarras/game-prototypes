using Assets.Scripts.Crawler.MapGen.Helpers;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Crawler.Maps.Entities;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.Quests.Settings;
using OxDb.SharedGame.Crawler.Worlds.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace OxDb.SharedGame.Crawler.Quests.Helpers
{
    public interface ICrawlerQuestTypeHelper : ISetupDictionaryItem<long>
    {
        Task SetupQuest(PartyData party, CrawlerWorld world, CrawlerMap startMap, MapLink targetMap, CrawlerNpc npc,
            CrawlerQuestType questType, IRandom rand, CancellationToken token);

        Task<string> ShowQuestStatus(PartyData party, long crawlerQuestId, bool fullDescription, bool showCurrentStatus, bool showNPC);
    }
}


