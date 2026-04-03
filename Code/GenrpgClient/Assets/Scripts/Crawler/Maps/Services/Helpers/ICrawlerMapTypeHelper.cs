using Assets.Scripts.Crawler.Maps.GameObjects;
using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Interfaces;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts.Crawler.Maps.Services.Helpers
{
    public interface ICrawlerMapTypeHelper : ISetupDictionaryItem<long>
    {

        Awaitable<CrawlerMapRoot> EnterMap(PartyData party, EnterCrawlerMapData mapData, CancellationToken token);

        int GetBlockingBits(CrawlerMap map, int startx, int startz, int endx, int endz, bool allowBuildingEntry);

    }
}


