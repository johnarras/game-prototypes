using Assets.Scripts.Crawler.Maps.GameObjects;
using Assets.Scripts.Crawler.Maps.Services.DrawCellHelpers;
using Assets.Scripts.Crawler.Services.CrawlerMaps;
using Genrpg.Shared.Crawler.Maps.Constants;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Worlds.Entities;
using Genrpg.Shared.HelperClasses;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logging.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts.Crawler.Maps.Services
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

        private OrderedSetupDictionaryContainer<Type, ICrawlerDrawCellHelper> _drawHelpers = new OrderedSetupDictionaryContainer<Type, ICrawlerDrawCellHelper>();

        const int ViewRadius = 8;
        public async Awaitable DrawNearbyMap(PartyData _party, CrawlerWorld _world, CrawlerMapRoot _crawlerMapRoot, CancellationToken token)
        {
            await Awaitable.MainThreadAsync();
            try
            {
                if (_crawlerMapRoot == null || _crawlerMapRoot.AssetBlocks.Any(a => !a.Value.IsReady()))
                {
                    return;
                }

                int centerX = (int)(_party.CurrPos.X);
                int centerZ = (int)(_party.CurrPos.Z);

                int nonLoopExtraRadius = _mapService.InDungeonMap() ? 1 : 0;

                int bigViewRadius = ViewRadius + 1;

                if (_crawlerMapRoot.Map.CrawlerMapTypeId == CrawlerMapTypes.Outdoors)
                {
                    bigViewRadius += 2;
                }

                foreach (ClientMapCell clientCell in _crawlerMapRoot.GetAllCells())
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

                        int mapCellX = (worldX + _crawlerMapRoot.Map.Width) % _crawlerMapRoot.Map.Width;
                        int mapCellZ = (worldZ + _crawlerMapRoot.Map.Height) % _crawlerMapRoot.Map.Height;

                        if (!_crawlerMapRoot.Map.HasFlag(CrawlerMapFlags.IsLooping) &&


                            (worldX < -nonLoopExtraRadius || worldX >= _crawlerMapRoot.Map.Width + nonLoopExtraRadius ||
                            worldZ < -nonLoopExtraRadius || worldZ >= _crawlerMapRoot.Map.Height + nonLoopExtraRadius))
                        {
                            continue;
                        }

                        ClientMapCell cell = _crawlerMapRoot.GetCellAtWorldPos(worldX, worldZ, true);

                        if ((offsetX >= ViewRadius + viewBufferSize ||
                            offsetZ >= ViewRadius + viewBufferSize))
                        {
                            _crawlerMapRoot.ReturnCell(cell);
                            continue;
                        }

                        cell.DidJustDraw = true;
                        cell.Content.transform.position = new Vector3(worldX * CrawlerMapConstants.XZBlockSize, 0, worldZ * CrawlerMapConstants.XZBlockSize);

                        if (!cell.DidInit)
                        {
                            cell.DidInit = true;
                            foreach (ICrawlerDrawCellHelper drawHelper in _drawHelpers.OrderedItems())
                            {
                                await drawHelper.DrawCell(_party, _world, _crawlerMapRoot, cell, worldX, worldZ, mapCellX, mapCellZ, token);
                            }
                        }
                    }
                }

                List<ClientMapCell> removeCells = new List<ClientMapCell>();
                foreach (ClientMapCell clientCell in _crawlerMapRoot.GetAllCells())
                {
                    if (!clientCell.DidJustDraw)
                    {
                        removeCells.Add(clientCell);
                    }
                }

                foreach (ClientMapCell clientCell in removeCells)
                {
                    _crawlerMapRoot.ReturnCell(clientCell);
                }
            }
            catch (Exception ex)
            {
                _logService.Exception(ex, "DrawNearbyMap");
            }
        }

        public string GetBuildingArtPrefix()
        {
            return "Default";
        }

    }
}
