using Assets.Scripts.Crawler.Maps.Services.GenerateMaps;
using Genrpg.Shared.Crawler.MapGen.Entities;
using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Worlds.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Crawler.MapGen.Helpers
{
    public interface ICrawlerMapGenHelper : ISetupDictionaryItem<long>
    {
        Task<NewCrawlerMap> Generate(PartyData party, CrawlerWorld world, CrawlerMapGenData crawlerMapGenData, CancellationToken token);
        NpcQuestMaps GetQuestMapsForNpc(PartyData party, CrawlerWorld world, CrawlerMap map, MapCellDetail npcDetail, IRandom rand);
    }
}
