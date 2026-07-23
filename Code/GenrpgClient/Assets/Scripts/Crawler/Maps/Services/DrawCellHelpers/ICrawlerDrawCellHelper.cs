using OxDb.Client.Crawler.Maps.GameObjects;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.Worlds.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OxDb.Client.Crawler.Maps.Services.DrawCellHelpers
{

    public enum ECrawlerDrawCellOrder
    {

        Walls = 100,

        Buildings = 300,

        Trees = 400,

        Props = 450,

        Encounters = 500,

        Riddles = 600,

        Stairs = 700,

        Teleport = 800,
    }

    public interface ICrawlerDrawCellHelper : IOrderedSetupDictionaryItem<ECrawlerDrawCellOrder>
    {
        ValueTask DrawCell(PartyData party, CrawlerWorld world, CrawlerMapRoot mapRoot, ClientMapCell cell, CancellationToken token);
    }
}


