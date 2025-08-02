using Assets.Scripts.Crawler.Maps.GameObjects;
using Assets.Scripts.Crawler.Maps.Loading;
using Assets.Scripts.Crawler.Maps.Services.DrawEntityHelpers;
using Genrpg.Shared.Client.Assets.Constants;
using Genrpg.Shared.Crawler.Maps.Constants;
using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Worlds.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.ProcGen.Settings.Trees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Crawler.Maps.Services.DrawCellHelpers
{
    public class TreeDrawCellHelper : BaseCrawlerDrawCellHelper
    {
        public override int Order => 400;

        public override async Awaitable DrawCell(PartyData party, CrawlerWorld world, CrawlerMapRoot mapRoot, ClientMapCell cell, int xpos, int zpos, int realCellX, int realCellZ, CancellationToken token)
        {
            long treeTypeId = mapRoot.Map.GetEntityId(cell.MapX, cell.MapZ, EntityTypes.Tree);
            if (treeTypeId > 0)
            {
                TreeType treeType = _gameData.Get<TreeTypeSettings>(null).Get(treeTypeId);

                if (treeType != null)
                {
                    int variation = 1;
                    if (treeType.VariationCount > 1)
                    {
                        variation = 1 + (cell.MapX * 31 + cell.MapZ * 47) % treeType.VariationCount;
                    }

                    CrawlerObjectLoadData loadData = new CrawlerObjectLoadData()
                    {
                        Angle = mapRoot.Map.Get(cell.MapX, cell.MapZ, CellIndex.Dir) * CrawlerMapConstants.DirToAngleMult,
                        Cell = cell,
                        MapRoot = mapRoot,
                        Seed = _mapService.GetMapCellHash(mapRoot.Map.IdKey, cell.MapX, cell.MapZ, treeTypeId * 17),

                    };

                    _assetService.LoadAssetInto(cell.Content, AssetCategoryNames.Trees, treeType.Art + variation, OnDownloadObject, loadData, token);

                }
            }

            await Task.CompletedTask;
        }
    }
}
