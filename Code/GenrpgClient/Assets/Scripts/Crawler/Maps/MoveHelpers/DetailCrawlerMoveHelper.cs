using OxDb.Client.Audio.ClientEvents;
using OxDb.Client.Crawler.Constants;
using OxDb.Client.Crawler.Maps.Services.Entities;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedGame.Crawler.Maps.Entities;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.States.Constants;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace OxDb.Client.Crawler.Maps.MoveHelpers
{
    public class DetailCrawlerMoveHelper : BaseCrawlerMoveHelper
    {
        public override ECrawlerMoveOrder HelperKey => ECrawlerMoveOrder.ProcessDetails;


        public override async Awaitable Execute(PartyData party, CrawlerMoveStatus status, CancellationToken token)
        {
            if (!status.MovedPosition || status.MoveIsStopped)
            {
                return;
            }


            MapCellDetail detail = status.MapRoot.Map.Details.FirstOrDefault(x => x.X == status.EX && x.Z == status.EZ);
            if (detail != null)
            {
                if (detail.EntityTypeId == EntityTypes.Map)
                {
                    _crawlerService.ChangeState(ECrawlerStates.EnterMap, token, detail);
                    status.MoveIsStopped = true;
                }
                else if (detail.EntityTypeId == EntityTypes.TeleportIn)
                {
                    if (status.SX != status.EX || status.SZ != status.EZ)
                    {
                        if (!_mapService.PartyHasVisited(party.CurrPos.MapId, status.EX, status.EZ, false))
                        {
                            _mapService.MarkCellVisitedAndCheckForCompletion(party.CurrPos.MapId, status.EX, status.EZ);
                            _dispatcher.Dispatch(new PlaySound(CrawlerAudio.TeleportActivate));
                            _mapService.MovePartyTo(party, detail.ToX, detail.ToZ, party.CurrPos.Rot, true, token);
                            return;
                        }
                        else
                        {
                            _crawlerService.ChangeState(ECrawlerStates.TeleportConfirmation, token, detail);
                            status.MoveIsStopped = true;
                        }
                    }
                }
            }

            await Task.CompletedTask;
        }
    }
}



