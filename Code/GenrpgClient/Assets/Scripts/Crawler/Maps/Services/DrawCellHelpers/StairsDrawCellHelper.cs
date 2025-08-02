using Assets.Scripts.Crawler.Maps.GameObjects;
using Assets.Scripts.Crawler.Maps.Loading;
using Assets.Scripts.Crawler.Maps.Services.DrawEntityHelpers;
using Genrpg.Shared.Client.Assets.Constants;
using Genrpg.Shared.Crawler.Maps.Constants;
using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Worlds.Entities;
using Genrpg.Shared.Entities.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Crawler.Maps.Services.DrawCellHelpers
{
    public class StairsDrawCellHelper : BaseCrawlerDrawCellHelper
    {
        public override int Order => 700;

        public override async Awaitable DrawCell(PartyData party, CrawlerWorld world, CrawlerMapRoot mapRoot, ClientMapCell cell, int xpos, int zpos, int realCellX, int realCellZ, CancellationToken token)
        {
            List<MapCellDetail> cellDetails = mapRoot.Map.Details.Where(d => d.X == cell.MapX && d.Z == cell.MapZ).ToList();
            if (mapRoot.Map.CrawlerMapTypeId == CrawlerMapTypes.Dungeon)
            {

                List<MapCellDetail> exitDetails = cellDetails.Where(d => d.X == cell.MapX && d.Z == cell.MapZ && d.EntityTypeId == EntityTypes.Map).ToList();

                if (exitDetails.Count > 0)
                {
                    MapCellDetail detail = exitDetails.First();
                    bool showDownStairs =
                        //mapRoot.Map.HasFlag(CrawlerMapFlags.NextLevelIsDown)  == 
                        (detail.EntityId == mapRoot.Map.IdKey + 1);

                    CrawlerObjectLoadData loadData = new CrawlerObjectLoadData()
                    {
                        Angle = mapRoot.Map.Get(cell.MapX, cell.MapZ, CellIndex.Dir) * CrawlerMapConstants.DirToAngleMult,
                        Cell = cell,
                        MapRoot = mapRoot,
                        Seed = _mapService.GetMapCellHash(mapRoot.Map.IdKey, cell.MapX, cell.MapZ, 1),

                    };

                    _assetService.LoadAssetInto(cell.Content, AssetCategoryNames.Props, (showDownStairs ? "StairsDown" : "StairsUp"), OnDownloadObject, loadData, token);
                }
            }


            await Task.CompletedTask;
        }
    }
}
