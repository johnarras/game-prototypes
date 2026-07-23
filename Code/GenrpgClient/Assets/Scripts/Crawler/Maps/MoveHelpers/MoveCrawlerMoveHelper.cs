using OxDb.Client.Crawler.Maps.Services.Entities;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace OxDb.Client.Crawler.Maps.MoveHelpers
{
    public class MoveCrawlerMoveHelper : BaseCrawlerMoveHelper
    {
        public override ECrawlerMoveOrder HelperKey => ECrawlerMoveOrder.ShowMove;


        public override async Awaitable Execute(PartyData party, CrawlerMoveStatus status, CancellationToken token)
        {

            if (status.KeyCode.RotationAmount == 0)
            {
                await _moveService.Move(status, status.KeyCode.ForwardAmount, status.KeyCode.RightAmount, token);
                status.MovedPosition = true;
            }
            else
            {
                await _moveService.Rot(status, status.KeyCode.RotationAmount, false, token);
                status.IsRotation = true;
            }
            await Task.CompletedTask;
        }
    }
}


