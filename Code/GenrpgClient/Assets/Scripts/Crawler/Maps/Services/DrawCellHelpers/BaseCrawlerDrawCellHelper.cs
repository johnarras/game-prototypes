using Assets.Scripts.Assets;
using Assets.Scripts.Crawler.Maps.GameObjects;
using Assets.Scripts.Crawler.Maps.Loading;
using Assets.Scripts.Crawler.Maps.Props;
using Assets.Scripts.Crawler.Maps.Services.DrawCellHelpers;
using Assets.Scripts.Crawler.Services.CrawlerMaps;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Worlds.Entities;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logging.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

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

        public virtual Type Key => GetType();
        
        public abstract Awaitable DrawCell(PartyData party, CrawlerWorld world, CrawlerMapRoot mapRoot, ClientMapCell cell, int xpos, int zpos, int realCellX, int realCellZ, CancellationToken token);

        protected virtual void OnDownloadObject(object obj, object data, CancellationToken token)
        {
            GameObject go = obj as GameObject;

            if (go == null)
            {
                return;
            }

            CrawlerObjectLoadData loadData = data as CrawlerObjectLoadData;

            if (loadData != null)
            {
                go.transform.eulerAngles = new Vector3(0, loadData.Angle, 0);
                loadData.Cell.Props.Add(go);

                go.name = go.name + "-" + loadData.Cell.MapX + "." + loadData.Cell.MapZ + "--" + go.transform.position / 8;
                CrawlerProp prop = _clientEntityService.GetComponent<CrawlerProp>(go);

                if (prop != null)
                {
                    prop.InitData(loadData.Cell.MapX, loadData.Cell.MapZ, loadData.MapRoot.Map);
                }
            }
        }
    }
}
