using Assets.Scripts.Setup.Interfaces;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.Crawler.Maps.Entities;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.Worlds.Entities;
using OxDb.SharedGame.Zones.Settings;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OxDb.SharedGame.Crawler.Maps.Services
{
    public interface ICrawlerWorldService : IInjectable, IGameTokenService
    {

        Task<CrawlerWorld> GenerateWorld(PartyData party);
        Task<CrawlerWorld> GetWorld(long worldId);
        CrawlerMap GetMap(long mapId);

        Task SaveWorld(CrawlerWorld world);

        Task<ZoneType> GetCurrentZone(PartyData party, long mapId = 0, int x = -1, int z = -1);
        Task<long> GetMapLevelAtPoint(CrawlerWorld world, long mapId, int x, int z);
        Task<long> GetMapLevelAtParty(PartyData party);
        CrawlerMap CreateMap(CrawlerMapGenData genData, int width, int height);
        Task<List<ZoneUnitSpawn>> GetSpawnsAtPoint(PartyData party, long mapId, int x, int z);
    }
}


