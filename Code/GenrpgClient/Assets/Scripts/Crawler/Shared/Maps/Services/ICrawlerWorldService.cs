using Genrpg.Shared.Client.Tokens;
using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Worlds.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Zones.Settings;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Genrpg.Shared.Crawler.Maps.Services
{
    public interface ICrawlerWorldService : IInjectable, IGameTokenService
    {

        Task<CrawlerWorld> GenerateWorld(PartyData party);
        Task<CrawlerWorld> GetWorld(long worldId);
        CrawlerMap GetMap(long mapId);

        Task SaveWorld(CrawlerWorld world);

        Task<ZoneType> GetCurrentZone(PartyData party, long mapId = 0, int x = -1, int z = -1);
        Task<int> GetMapLevelAtPoint(CrawlerWorld world, long mapId, int x, int z);
        Task<int> GetMapLevelAtParty(PartyData party);
        CrawlerMap CreateMap(CrawlerMapGenData genData, int width, int height);
        Task<List<ZoneUnitSpawn>> GetSpawnsAtPoint(PartyData party, long mapId, int x, int z);
    }
}
