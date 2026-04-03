using Assets.Scripts.Buildings;
using Assets.Scripts.Crawler.Maps.GameObjects;
using Assets.Scripts.Crawler.Maps.Loading;
using Assets.Scripts.Crawler.Maps.Services.DrawEntityHelpers;
using Genrpg.Shared.Buildings.Settings;
using Genrpg.Shared.Crawler.Maps.Constants;
using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Worlds.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Utils;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Crawler.Maps.Services.DrawCellHelpers
{

    public class BuildingDrawCellHelper : BaseCrawlerDrawCellHelper
    {
        public override int Order => 300;

        public override async Awaitable DrawCell(PartyData party, CrawlerWorld world, CrawlerMapRoot mapRoot, ClientMapCell cell, int xpos, int zpos, int realCellX, int realCellZ, CancellationToken token)
        {
            long buildingTypeId = mapRoot.Map.GetEntityId(cell.MapX, cell.MapZ, EntityTypes.Building);

            if (buildingTypeId > 0)
            {
                BuildingType btype = _gameData.Get<BuildingSettings>(null).Get(buildingTypeId);

                if (btype != null)
                {
                    string suffix = "";

                    CrawlerObjectLoadData loadData = new CrawlerObjectLoadData()
                    {
                        Cell = cell,
                        Data = btype,
                        Angle = mapRoot.Map.Get(cell.MapX, cell.MapZ, CellIndex.Dir) * CrawlerMapConstants.DirToAngleMult,
                        MapRoot = mapRoot,
                        Seed = cell.MapX * 31 + cell.MapZ * 97 + world.Seed / 11 + mapRoot.Map.ArtSeed / 3 + mapRoot.Map.IdKey / 7,
                    };

                    string buildingArtName = btype.Art + suffix;

                    if (mapRoot.CityAssets != null)
                    {
                        IRandom rand = new MyRandom(loadData.Seed);
                        WeightedCrawlerBuilding wcb = RandUtils.GetRandomFloatElement(mapRoot.CityAssets.Buildings, rand);
                        await ShowBuilding(wcb.Building, wcb.Mats, cell.Content, loadData);
                    }
                }
            }


            await Task.CompletedTask;
        }
        protected async Awaitable ShowBuilding(CrawlerBuilding buildingIn, BuildingMats mats, object parent, CrawlerObjectLoadData loadData)
        {

            if (loadData == null || loadData.Cell == null || loadData.Data == null || loadData.MapRoot == null)
            {
                return;
            }
            CrawlerBuilding crawlerBuilding = _clientEntityService.FullInstantiate(buildingIn);
            _clientEntityService.AddToParent(crawlerBuilding, parent);

            await crawlerBuilding.SetData(loadData.Data as BuildingType, loadData.Seed, loadData.MapRoot, loadData.Cell, mats);
            crawlerBuilding.transform.eulerAngles = new Vector3(0, loadData.Angle, 0);
            crawlerBuilding.transform.localScale = Vector3.one;

            await Task.CompletedTask;
        }

    }
}


