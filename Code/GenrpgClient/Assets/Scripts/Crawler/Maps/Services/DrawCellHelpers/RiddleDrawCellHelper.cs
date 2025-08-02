using Assets.Scripts.Crawler.Maps.GameObjects;
using Assets.Scripts.Crawler.Maps.Loading;
using Assets.Scripts.Crawler.Maps.Services.DrawEntityHelpers;
using Genrpg.Shared.Client.Assets.Constants;
using Genrpg.Shared.Crawler.Maps.Constants;
using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Worlds.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Riddles.Services;
using Genrpg.Shared.Riddles.Settings;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Crawler.Maps.Services.DrawCellHelpers
{
    public class RiddleDrawCellHelper : BaseCrawlerDrawCellHelper
    {

        private IRiddleService _riddleService = null;
        public override int Order => 600;

        public override async Awaitable DrawCell(PartyData party, CrawlerWorld world, CrawlerMapRoot mapRoot, ClientMapCell cell, int xpos, int zpos, int realCellX, int realCellZ, CancellationToken token)
        {
            int riddleIndex = mapRoot.Map.GetEntityId(cell.MapX, cell.MapZ, EntityTypes.Riddle);

            if (riddleIndex > 0 && !party.RiddlesCompleted.HasBit(mapRoot.Map.IdKey))
            {
                RiddleType riddleType = _gameData.Get<RiddleTypeSettings>(_gs.ch).Get(mapRoot.Map.RiddleHints?.RiddleTypeId ?? 0);

                if (riddleType != null && _riddleService.ShouldDrawProp(party, realCellX, realCellZ))
                {
                    CrawlerObjectLoadData loadData = new CrawlerObjectLoadData()
                    {
                        Angle = mapRoot.Map.Get(cell.MapX, cell.MapZ, CellIndex.Dir) * CrawlerMapConstants.DirToAngleMult,
                        Cell = cell,
                        MapRoot = mapRoot,
                        Seed = _mapService.GetMapCellHash(mapRoot.Map.IdKey, cell.MapX, cell.MapZ, riddleIndex * 13),

                    };

                    _assetService.LoadAssetInto(cell.Content, AssetCategoryNames.Props, riddleType.Art, OnDownloadObject, loadData, token);
                }
            }

            await Task.CompletedTask;
        }

        protected override void OnDownloadObject(object obj, object data, CancellationToken token)
        {
            base.OnDownloadObject(obj, data, token);
            _riddleService.SetPropPosition(obj, data, token);
        }
    }
}
