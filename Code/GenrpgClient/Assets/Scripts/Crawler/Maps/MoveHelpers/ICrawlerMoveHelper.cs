using OxDb.Client.Crawler.Maps.Services.Entities;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using System;
using System.Threading;
using UnityEngine;

namespace OxDb.Client.Crawler.Maps.MoveHelpers
{
    public enum ECrawlerMoveOrder
    {

        ShowMove = 100,

        TryEnterBuilding = 200,

        Riddles = 250,

        QuestItem = 275,

        ProcessDetails = 300,

        MapEncounters = 400,

        RandomCombat = 500,

        GraphicalMove = 600,

        UpdateTime = 700,

        ApplyMagic = 800,

        UpdateUI = 900,

        FinishMove = 1000,

        ShowMinimap = 1100,
    }


    public interface ICrawlerMoveHelper : IOrderedSetupDictionaryItem<ECrawlerMoveOrder>
    {
        // Must be awaitable since some of this does happen over time.
        Awaitable Execute(PartyData party, CrawlerMoveStatus status, CancellationToken token);
    }
}


