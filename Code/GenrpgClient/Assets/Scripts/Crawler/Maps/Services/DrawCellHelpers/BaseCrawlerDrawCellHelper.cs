using Assets.Scripts.Crawler.Maps.GameObjects;
using Assets.Scripts.Crawler.Maps.Services.DrawCellHelpers;
using Assets.Scripts.Crawler.Services.CrawlerMaps;
using Assets.Scripts.GameObjects;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.Worlds.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.Crawler.Maps.Services.DrawEntityHelpers
{
    public abstract class BaseCrawlerDrawCellHelper : ICrawlerDrawCellHelper
    {

        protected IClientEntityService _clientEntityService = null;
        protected IGameData _gameData = null;
        protected IClientGameState _gs = null;
        protected ILogService _logService = null;
        protected IAssetService _assetService = null;
        protected ICrawlerMapService _mapService = null;

        public abstract int Order { get; }

        public virtual Type HelperKey => GetType();

        public abstract ValueTask DrawCell(PartyData party, CrawlerWorld world, CrawlerMapRoot mapRoot, ClientMapCell cell, int xpos, int zpos, int realCellX, int realCellZ, CancellationToken token);

    }
}


