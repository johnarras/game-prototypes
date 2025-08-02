using Assets.Scripts.Crawler.Maps.Services.GenerateMaps;
using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Quests.Settings;
using Genrpg.Shared.Crawler.Worlds.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Crawler.Quests.Helpers
{
    public interface ICrawlerQuestTypeHelper : ISetupDictionaryItem<long>
    {
        Task SetupQuest(PartyData party, CrawlerWorld world, CrawlerMap startMap, MapLink targetMap, CrawlerNpc npc, 
            CrawlerQuestType questType, IRandom rand, CancellationToken token);

        Task<string> ShowQuestStatus(PartyData party, long crawlerQuestId, bool fullDescription,  bool showCurrentStatus, bool showNPC);
    }
}
