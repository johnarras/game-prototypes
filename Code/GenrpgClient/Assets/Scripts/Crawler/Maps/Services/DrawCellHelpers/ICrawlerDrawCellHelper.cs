using Assets.Scripts.Crawler.Maps.GameObjects;
using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Worlds.Entities;
using Genrpg.Shared.Interfaces;
using System;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts.Crawler.Maps.Services.DrawCellHelpers
{
    public interface ICrawlerDrawCellHelper : IOrderedSetupDictionaryItem<Type>
    {
        Awaitable DrawCell(PartyData party, CrawlerWorld world, CrawlerMapRoot mapRoot, ClientMapCell cell, int worldX, int worldZ, int mapX, int mapZ, CancellationToken token);
    }
}
