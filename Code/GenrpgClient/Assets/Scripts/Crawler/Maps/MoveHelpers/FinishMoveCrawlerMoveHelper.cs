using Assets.Scripts.Crawler.Maps.Services.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Crawler.Maps.MoveHelpers
{
    public class FinishMoveCrawlerMoveHelper : BaseCrawlerMoveHelper
    {
        public override int Order => 1000;

        public override async Awaitable Execute(PartyData party, CrawlerMoveStatus status, CancellationToken token)
        {
            _moveService.FinishMove(status);
            await Task.CompletedTask;
        }
    }
}
