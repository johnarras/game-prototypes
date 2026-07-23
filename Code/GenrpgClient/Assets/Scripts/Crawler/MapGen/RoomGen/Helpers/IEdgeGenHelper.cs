
using OxDb.Client.Crawler.MapGen.Helpers;
using OxDb.Client.Crawler.MapGen.RoomGen.Entities;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.Crawler.Maps.Entities;
using System.Threading.Tasks;

namespace OxDb.Client.Crawler.MapGen.RoomGen.Helpers
{
    public interface IEdgeGenHelper : ISetupDictionaryItem<long>
    {
        ValueTask GenerateEdge(RoomEdgeGenArgs edgeArgs, CrawlerMapGenData genData, DungeonLevelGenArgs levelArgs);
    }
}
