using Assets.Scripts.Assets;
using Assets.Scripts.Crawler.Maps.GameObjects;
using Assets.Scripts.Crawler.Maps.Loading;
using Assets.Scripts.Crawler.Services.CrawlerMaps;
using Genrpg.Shared.Client.Assets.Constants;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Worlds.Entities;
using Genrpg.Shared.GameSettings;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts.Crawler.Maps.EncounterHelpers
{
    public abstract class BaseClientMapEncounterHelper : IClientMapEncounterHelper
    {
        protected IGameData _gameData = null;
        protected IClientGameState _gs = null;
        protected ICrawlerMapService _mapService = null;
        protected IAssetService _assetService = null;
        protected IClientEntityService _clientEntityService = null;

        public abstract long HelperKey { get; }
        public abstract Awaitable DrawCell(PartyData party, CrawlerWorld world, CrawlerMapRoot mapRoot, ClientMapCell cell, int x, int z, CancellationToken token);
        protected abstract void AfterDownloadProp(GameObject prop, CrawlerObjectLoadData args);

        protected void LoadPropAtCell(CrawlerMapRoot mapRoot, ClientMapCell cell, string prefabName, int x, int z, object data, CancellationToken token)
        {
            CrawlerObjectLoadData args = new CrawlerObjectLoadData()
            {
                MapRoot = mapRoot,
                Cell = cell,
                Data = data,
            };
            _assetService.LoadAssetInto(cell.Content, AssetCategoryNames.Props, prefabName, OnDownloadProp, token, args);
        }

        protected void OnDownloadProp(GameObject go, CrawlerObjectLoadData args, CancellationToken token)
        {
            args.Cell.Props.Add(go);

            AfterDownloadProp(go, args);
        }
    }
}
