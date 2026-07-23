using OxDb.Client.Crawler.Maps.GameObjects;
using OxDb.Client.Crawler.Maps.Services.Entities;
using OxDb.Client.Crawler.Services.CrawlerMaps;
using OxDb.Client.GameObjects;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedGame.Crawler.Maps.Entities;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.States.Services;
using OxDb.SharedGame.Crawler.Worlds.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace OxDb.Client.Crawler.Maps.EncounterHelpers
{
    public abstract class BaseClientMapEncounterHelper : IClientMapEncounterHelper
    {
        protected IGameData _gameData = null;
        protected IClientGameState _gs = null;
        protected ICrawlerMapService _mapService = null;
        protected IAssetService _assetService = null;
        protected IClientEntityService _clientEntityService = null;
        protected ICrawlerService _crawlerService = null;

        public abstract long HelperKey { get; }
        public abstract ValueTask DrawCell(PartyData party, CrawlerWorld world, CrawlerMapRoot mapRoot, ClientMapCell cell, int x, int z, CancellationToken token);
        public abstract ValueTask OnEnterCell(PartyData party, CrawlerMap map, CrawlerMapStatus mapStatus, CrawlerMoveStatus moveStatus, CancellationToken token);

    }
}


