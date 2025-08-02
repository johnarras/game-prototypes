
using Genrpg.Shared.Crawler.Maps.Constants;
using Genrpg.Shared.Crawler.Maps.Entities;
using Assets.Scripts.Crawler.Maps.GameObjects;
using Assets.Scripts.Crawler.Services.CrawlerMaps;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Interfaces;
using System.Threading;
using UnityEngine;
using Genrpg.Shared.Crawler.Worlds.Entities;

namespace Assets.Scripts.Crawler.Maps.Services.Helpers
{
    public interface ICrawlerMapTypeHelper : ISetupDictionaryItem<long>
    {

        Awaitable<CrawlerMapRoot> EnterMap(PartyData party, EnterCrawlerMapData mapData, CancellationToken token);

        int GetBlockingBits(CrawlerMap map, int startx, int startz, int endx, int endz, bool allowBuildingEntry);

    }
}
