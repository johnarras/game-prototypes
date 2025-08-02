using Assets.Scripts.Crawler.Maps.Services.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Interfaces;
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
