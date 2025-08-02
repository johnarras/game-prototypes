using Assets.Scripts.Crawler.Maps.Services.Entities;
using Genrpg.Shared.Crawler.Combat.Entities;
using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.Worlds.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.MapServer.Entities;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Crawler.Maps.MoveHelpers
{
    public class DetailCrawlerMoveHelper : BaseCrawlerMoveHelper
    {
        public override int Order => 300;

        public override async Awaitable Execute(PartyData party, CrawlerMoveStatus status, CancellationToken token)
        {
            if (!status.MovedPosition || status.MoveIsComplete)
            {
                return;
            }


            MapCellDetail detail = status.MapRoot.Map.Details.FirstOrDefault(x => x.X == status.EX && x.Z == status.EZ);
            if (detail != null)
            {
                if (detail.EntityTypeId == EntityTypes.Map)
                {
                    _crawlerService.ChangeState(ECrawlerStates.EnterMap, token, detail);
                    status.MoveIsComplete = true;
                }
                else if (detail.EntityTypeId == EntityTypes.TeleportIn)
                {
                    if (status.SX != status.EX || status.SZ != status.EZ)
                    {
                        if (!_mapService.PartyHasVisited(party.CurrPos.MapId, status.EX, status.EZ, false))
                        {
                            _mapService.MarkCellVisitedAndCheckForCompletion(party.CurrPos.MapId, status.EX, status.EZ);
                            _mapService.MovePartyTo(party, detail.ToX, detail.ToZ, party.CurrPos.Rot, true, token);
                            return;
                        }
                        else
                        {
                            _crawlerService.ChangeState(ECrawlerStates.TeleportConfirmation, token, detail);
                            status.MoveIsComplete = true;
                        }
                    }
                }
            }

            await Task.CompletedTask;
        }
    }
}

