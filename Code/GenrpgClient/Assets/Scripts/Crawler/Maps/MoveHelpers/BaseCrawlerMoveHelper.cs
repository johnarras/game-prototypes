using OxDb.Client.Crawler.Maps.Services;
using OxDb.Client.Crawler.Maps.Services.Entities;
using OxDb.Client.Crawler.Services.CrawlerMaps;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedGame.Crawler.Options.Services;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.Party.Services;
using OxDb.SharedGame.Crawler.States.Services;
using OxDb.SharedGame.Crawler.Stats.Services;
using System;
using System.Threading;
using UnityEngine;

namespace OxDb.Client.Crawler.Maps.MoveHelpers
{
    public abstract class BaseCrawlerMoveHelper : ICrawlerMoveHelper
    {

        protected ICrawlerMoveService _moveService = null;
        protected ICrawlerMapService _mapService = null;
        protected ICrawlerService _crawlerService = null;
        protected ICrawlerWorldService _worldService = null;
        protected IGameData _gameData = null;
        protected IClientGameState _gs = null;
        protected IDispatcher _dispatcher = null;
        protected ICrawlerStatService _crawlerStatService = null;
        protected IPartyService _partyService = null;
        protected ILogService _logService = null;
        protected ICrawlerOptionsService _optionService = null;

        public abstract ECrawlerMoveOrder HelperKey { get; }
        public abstract Awaitable Execute(PartyData party, CrawlerMoveStatus status, CancellationToken token);
    }
}


