using Assets.Scripts.Crawler.Maps.GameObjects;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.Crawler.Maps.Entities;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
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


