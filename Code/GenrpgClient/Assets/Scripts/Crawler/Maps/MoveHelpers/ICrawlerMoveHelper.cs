using Assets.Scripts.Crawler.Maps.Services.Entities;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using System;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts.Crawler.Maps.MoveHelpers
{
    public interface ICrawlerMoveHelper : IOrderedSetupDictionaryItem<Type>
    {
        Awaitable Execute(PartyData party, CrawlerMoveStatus status, CancellationToken token);
    }
}


