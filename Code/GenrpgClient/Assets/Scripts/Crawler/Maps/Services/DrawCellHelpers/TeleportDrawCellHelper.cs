using Assets.Scripts.Crawler.Maps.GameObjects;
using Assets.Scripts.Crawler.Maps.Loading;
using Assets.Scripts.Crawler.Maps.Services.DrawEntityHelpers;
using Genrpg.Shared.Client.Assets.Constants;
using Genrpg.Shared.Crawler.Maps.Constants;
using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Worlds.Entities;
using Genrpg.Shared.Entities.Constants;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Crawler.Maps.Services.DrawCellHelpers
{
    public class TeleportDrawCellHelper : BaseCrawlerDrawCellHelper
    {
        public override int Order => 800;

        public override async Awaitable DrawCell(PartyData party, CrawlerWorld world, CrawlerMapRoot mapRoot, ClientMapCell cell, int xpos, int zpos, int realCellX, int realCellZ, CancellationToken token)
        {
            MapCellDetail teleportDetail = mapRoot.Map.Details.FirstOrDefault(d => d.X == cell.MapX && d.Z == cell.MapZ &&
            d.EntityTypeId == EntityTypes.TeleportIn);
            if (teleportDetail != null)
            {
                CrawlerObjectLoadData loadData = new CrawlerObjectLoadData()
                {
                    Angle = mapRoot.Map.Get(cell.MapX, cell.MapZ, CellIndex.Dir) * CrawlerMapConstants.DirToAngleMult,
                    Cell = cell,
                    MapRoot = mapRoot,
                    Seed = _mapService.GetMapCellHash(mapRoot.Map.IdKey, cell.MapX, cell.MapZ, 17),

                };
                _assetService.LoadAssetInto(cell.Content, AssetCategoryNames.Props, "TeleportIn", OnDownloadObject, loadData, token);
            }
            await Task.CompletedTask;
        }
    }
}
