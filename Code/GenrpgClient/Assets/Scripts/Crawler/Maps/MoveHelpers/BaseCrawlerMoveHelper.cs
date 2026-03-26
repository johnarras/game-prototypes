using Assets.Scripts.Crawler.Maps.Services.Entities;
using Assets.Scripts.Crawler.Services.CrawlerMaps;
using Assets.Scripts.Core;
using Genrpg.Shared.Crawler.Maps.Services;
using Genrpg.Shared.Crawler.Options.Services;
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


