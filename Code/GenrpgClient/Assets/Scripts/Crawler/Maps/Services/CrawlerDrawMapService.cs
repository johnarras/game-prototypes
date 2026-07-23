using OxDb.Client.Assets.Scripts.Crawler.Maps.Services;
using OxDb.Client.Awaitables;
using OxDb.Client.Crawler.Maps.GameObjects;
using OxDb.Client.Crawler.Maps.Services.DrawCellHelpers;
using OxDb.Client.Crawler.Services.CrawlerMaps;
using OxDb.SharedCore.HelperClasses;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedGame.Crawler.Maps.Constants;
using OxDb.SharedGame.Crawler.Maps.Entities;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.Worlds.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace OxDb.Client.Crawler.Maps.Services
{
    public interface ICrawlerDrawMapService : IInjectable
    {

        string GetBuildingArtPrefix();
        Awaitable DrawNearbyMap(PartyData _party, CrawlerWorld _world, CrawlerMapRoot _crawlerMapRoot, CancellationToken token);

    }


    public class CrawlerDrawMapService : ICrawlerDrawMapService
    {
        private ICrawlerMapService _mapService = null;
        private ILogService _logService = null;
        private IAwaitableService _awaitableService = null;
        private ICrawlerPropService _propService = null;

        private OrderedSetupDictionaryContainer<ECrawlerDrawCellOrder, ICrawlerDrawCellHelper> _drawHelpers = new OrderedSetupDictionaryContainer<ECrawlerDrawCellOrder, ICrawlerDrawCellHelper>();

        public const int ViewRadius = 8;
        public async Awaitable DrawNearbyMap(PartyData party, CrawlerWorld world, CrawlerMapRoot mapRoot, CancellationToken token)
        {
            try
            {
                if (mapRoot == null || !mapRoot.AssetsAreReady())
                {
                    return;
                }


                int centerX = (int)(party.CurrPos.X);
                int centerZ = (int)(party.CurrPos.Z);

                int nonLoopExtraRadius = _mapService.InDungeonMap() ? 1 : 0;

                int bigViewRadius = ViewRadius + 1;

                if (mapRoot.Map.CrawlerMapTypeId == CrawlerMapTypes.Outdoors)
                {
                    bigViewRadius += 2;
                }

                foreach (ClientMapCell clientCell in mapRoot.GetAllCells())
                {
                    clientCell.DidJustDraw = false;
                }

                int viewBufferSize = bigViewRadius + 1;

                for (int worldX = centerX - bigViewRadius; worldX <= centerX + bigViewRadius; worldX++)
                {
                    int offsetX = Math.Abs(worldX - centerX);
                    for (int worldZ = centerZ - bigViewRadius; worldZ <= centerZ + bigViewRadius; worldZ++)
                    {
                        int offsetZ = Math.Abs((int)(worldZ - centerZ));

                        if ((worldX < -nonLoopExtraRadius || worldX >= mapRoot.Map.Width + nonLoopExtraRadius ||
                            worldZ < -nonLoopExtraRadius || worldZ >= mapRoot.Map.Height + nonLoopExtraRadius))
                        {
                            continue;
                        }

                        ClientMapCell cell = mapRoot.GetCellAtWorldPos(worldX, worldZ, true, true);

                        if ((offsetX >= ViewRadius + viewBufferSize ||
                            offsetZ >= ViewRadius + viewBufferSize))
                        {
                            mapRoot.ReturnCell(cell);
                            continue;
                        }

                        cell.DidJustDraw = true;
                        cell.Content.transform.position = new Vector3(worldX * mapRoot.XZBlockSize, 0, worldZ * mapRoot.XZBlockSize);

                        if (!cell.DidInit)
                        {
                            cell.DidInit = true;
                            foreach (ICrawlerDrawCellHelper drawHelper in _drawHelpers.OrderedItems())
                            {
                                await drawHelper.DrawCell(party, world, mapRoot, cell, token);
                            }
                        }
                    }
                }

                List<ClientMapCell> removeCells = new List<ClientMapCell>();
                foreach (ClientMapCell clientCell in mapRoot.GetAllCells())
                {
                    if (!clientCell.DidJustDraw)
                    {
                        removeCells.Add(clientCell);
                    }
                }

                foreach (ClientMapCell clientCell in removeCells)
                {
                    mapRoot.ReturnCell(clientCell);
                }
            }
            catch (Exception ex)
            {
                _logService.Exception(ex, "DrawNearbyMap");
            }

            if (!mapRoot.DidDrawEdgeProps)
            {
                _awaitableService.ForgetAwaitable(_propService.DrawEdgeProps(party, world, mapRoot, token));
            }
        }

        public string GetBuildingArtPrefix()
        {
            return "Default";
        }
    }
}


