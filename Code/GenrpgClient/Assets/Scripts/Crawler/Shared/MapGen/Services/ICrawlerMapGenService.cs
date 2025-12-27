using Genrpg.Shared.Crawler.MapGen.Helpers;
using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Worlds.Entities;
using Genrpg.Shared.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Crawler.MapGen.Services
{
    public interface ICrawlerMapGenService : IInitializable
    {
        ICrawlerMapGenHelper GetGenHelper(long mapType);
        Task<CrawlerMap> Generate(PartyData party, CrawlerWorld world, CrawlerMapGenData genData, CancellationToken token);
        void OneWayLink(CrawlerWorld world, long fromMapId, int fromX, int fromZ, long toMapId, int toX, int toZ);
    }
}


