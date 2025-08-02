using Assets.Scripts.Crawler.Maps.Services.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Crawler.Maps.MoveHelpers
{
    public class MoveCrawlerMoveHelper : BaseCrawlerMoveHelper
    {
        public override int Order => 100;

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
