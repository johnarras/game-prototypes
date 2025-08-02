using Assets.Scripts.Crawler.Maps.Services.Entities;
using Genrpg.Shared.Buildings.Settings;
using Genrpg.Shared.Crawler.Maps.Constants;
using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Crawler.States.StateHelpers;
using Genrpg.Shared.Entities.Constants;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Crawler.Maps.MoveHelpers
{
    public class TryEnterBuildingCrawlerMoveHelper : BaseCrawlerMoveHelper
    {
        public override int Order => 200;

        public override async Awaitable Execute(PartyData party, CrawlerMoveStatus status, CancellationToken token)
        {

            if (status.BlockBits == WallTypes.Building)
            {
                int ex = status.EX;
                int ez = status.EZ;
                if (TryEnterBuilding(party, status, token))
                {
                    _mapService.MarkCellVisitedAndCheckForCompletion(status.MapRoot.Map.IdKey, ex, ez);
                    status.MoveIsComplete = true;
                    return;
                }
            }
            await Task.CompletedTask;
        }


        private bool TryEnterBuilding(PartyData _party, CrawlerMoveStatus status, CancellationToken token)
        {

            ForcedNextState nextState = TryGetNextForcedState(status.MapRoot.Map, status.EX, status.EZ);

            if (nextState != null)
            {
                _party.CurrPos.X = status.SX;
                _party.CurrPos.Z = status.SZ;
                status.EX = status.SX;
                status.EZ = status.SZ;
                status.MoveIsComplete = true;
                    
                _moveService.SetFullRot(_party.CurrPos.Rot + 180);
                _mapService.UpdateCameraPos(token);
                _crawlerService.ChangeState(nextState.NextState, token, nextState.Detail);
                return true;
            }

            return false;
        }

        public ForcedNextState TryGetNextForcedState(CrawlerMap map, int ex, int ez)
        {
            byte buildingId = map.GetEntityId(ex, ez, EntityTypes.Building);

            BuildingType btype = _gameData.Get<BuildingSettings>(null).Get(buildingId);

            if (btype == null)
            {
                return null;
            }

            List<MapCellDetail> details = map.Details.Where(d => d.X == ex && d.Z == ez).ToList();

            List<IStateHelper> helpers = _crawlerService.GetAllStateHelpers();

            foreach (MapCellDetail detail in details)
            {
                IStateHelper helper = helpers.FirstOrDefault(x => x.TriggerDetailEntityTypeId() == detail.EntityTypeId);
                if (helper != null)
                {
                    return new ForcedNextState() { NextState = helper.Key, Detail = detail };
                }
            }

            IStateHelper buildingHelper = helpers.FirstOrDefault(x => x.TriggerBuildingId() == buildingId);

            if (buildingHelper != null)
            {
                return new ForcedNextState() { NextState = buildingHelper.Key };
            }

            return null;
        }

    }
}
