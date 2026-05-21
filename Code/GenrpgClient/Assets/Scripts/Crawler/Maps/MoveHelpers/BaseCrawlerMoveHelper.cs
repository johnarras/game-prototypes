using Assets.Scripts.Core;
using Assets.Scripts.Crawler.Maps.Services.Entities;
using Assets.Scripts.Crawler.Services.CrawlerMaps;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedGame.Crawler.Maps.Services;
using OxDb.SharedGame.Crawler.Options.Services;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.Party.Services;
using OxDb.SharedGame.Crawler.States.Services;
using OxDb.SharedGame.Crawler.Stats.Services;
using System;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts.Crawler.Maps.MoveHelpers
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
        protected IClientRandom _rand = null;
        protected ICrawlerStatService _crawlerStatService = null;
        protected IPartyService _partyService = null;
        protected ILogService _logService = null;
        protected ICrawlerOptionsService _optionService = null;

        public abstract int Order { get; }
        public Type HelperKey => GetType();
        public abstract Awaitable Execute(PartyData party, CrawlerMoveStatus status, CancellationToken token);
    }
}


