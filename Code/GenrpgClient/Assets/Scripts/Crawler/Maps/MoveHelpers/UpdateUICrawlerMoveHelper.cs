using Assets.Scripts.Crawler.Maps.Services.Entities;
using Genrpg.Shared.Crawler.GameEvents;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Crawler.Maps.MoveHelpers
{
    public class UpdateUICrawlerMoveHelper : BaseCrawlerMoveHelper
    {
        public override int Order => 900;

        public override async Awaitable Execute(PartyData party, CrawlerMoveStatus status, CancellationToken token)
        {
            await Task.CompletedTask;
            _dispatcher.Dispatch(new UpdateCrawlerUI());
        }
    }
}
