using Assets.Scripts.Crawler.Maps.GameObjects;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.Worlds.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.Crawler.Maps.Services.DrawCellHelpers
{
    public interface ICrawlerDrawCellHelper : IOrderedSetupDictionaryItem<Type>
    {
        ValueTask DrawCell(PartyData party, CrawlerWorld world, CrawlerMapRoot mapRoot, ClientMapCell cell, int worldX, int worldZ, int mapX, int mapZ, CancellationToken token);
    }
}


