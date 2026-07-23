using OxDb.Client.Crawler.Maps.GameObjects;
using OxDb.Client.Crawler.Maps.Services.DrawCellHelpers;
using OxDb.Client.Crawler.Services.CrawlerMaps;
using OxDb.Client.GameObjects;
using OxDb.SharedCore.Entities.Services;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.Worlds.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OxDb.Client.Crawler.Maps.Services.DrawEntityHelpers
{
    public abstract class BaseCrawlerDrawCellHelper : ICrawlerDrawCellHelper
    {

        protected IClientEntityService _clientEntityService = null;
        protected IGameData _gameData = null;
        protected IClientGameState _gs = null;
        protected ILogService _logService = null;
        protected IAssetService _assetService = null;
        protected ICrawlerMapService _mapService = null;
        protected IEntityService _entityService = null;

        public abstract ECrawlerDrawCellOrder HelperKey { get; }

        public abstract ValueTask DrawCell(PartyData party, CrawlerWorld world, CrawlerMapRoot mapRoot, ClientMapCell cell, CancellationToken token);

    }
}


