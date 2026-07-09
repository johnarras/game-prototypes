using Assets.Scripts.Crawler.MapGen.Helpers;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.Crawler.Maps.Entities;
using System.Threading.Tasks;

namespace Assets.Scripts.Crawler.MapGen.DungeonGen.Helpers
{
    public interface IDungeonGenHelper : ISetupDictionaryItem<long>
    {
        ValueTask<bool> GenerateLevel(CrawlerMapGenData genData, DungeonLevelGenArgs args);
    }
}
