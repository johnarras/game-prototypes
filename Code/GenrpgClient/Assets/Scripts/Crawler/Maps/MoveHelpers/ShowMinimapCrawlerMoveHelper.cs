using Assets.Scripts.Crawler.Maps.Services.Entities;
using Genrpg.Shared.Crawler.GameEvents;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Crawler.Maps.MoveHelpers
{

    public class ShowMinimapCrawlerMoveHelper : BaseCrawlerMoveHelper
    {
        public override int Order => 1100;

        public override async Awaitable Execute(PartyData party, CrawlerMoveStatus status, CancellationToken token)
        {
            _dispatcher.Dispatch(new ShowPartyMinimap() { Party = party });
            await Task.CompletedTask;
        }
    }
}
