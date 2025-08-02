using Assets.Scripts.Crawler.Maps.Services.Entities;
using Assets.Scripts.Crawler.Services.CrawlerMaps;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Crawler.Maps.Services;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Party.Services;
using Genrpg.Shared.Crawler.States.Services;
using Genrpg.Shared.Crawler.Stats.Services;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Logging.Interfaces;
using System;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts.Crawler.Maps.MoveHelpers
{
    public abstract class BaseCrawlerMoveHelper : ICrawlerMoveHelper
    {

        protected ICrawlerMoveService _moveService;
        protected ICrawlerMapService _mapService;
        protected ICrawlerService _crawlerService;
        protected ICrawlerWorldService _worldService;
        protected IGameData _gameData;
        protected IClientGameState _gs;
        protected IDispatcher _dispatcher;
        protected IClientRandom _rand;
        protected ICrawlerStatService _crawlerStatService;
        protected IPartyService _partyService;
        protected ILogService _logService;

        public abstract int Order { get; }
        public Type Key => GetType();
        public abstract Awaitable Execute(PartyData party, CrawlerMoveStatus status, CancellationToken token);
    }
}
