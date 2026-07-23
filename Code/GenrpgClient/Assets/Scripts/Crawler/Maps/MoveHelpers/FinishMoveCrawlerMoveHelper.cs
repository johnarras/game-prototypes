using OxDb.Client.Crawler.Maps.Services.Entities;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace OxDb.Client.Crawler.Maps.MoveHelpers
{
    public class FinishMoveCrawlerMoveHelper : BaseCrawlerMoveHelper
    {
        public override ECrawlerMoveOrder HelperKey => ECrawlerMoveOrder.FinishMove;


        public override async Awaitable Execute(PartyData party, CrawlerMoveStatus status, CancellationToken token)
        {
            _moveService.FinishMove(status);
            await Task.CompletedTask;
        }
    }
}


