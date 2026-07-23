using OxDb.Client.Crawler.Maps.Services.Entities;
using OxDb.SharedGame.Crawler.GameEvents;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace OxDb.Client.Crawler.Maps.MoveHelpers
{

    public class ShowMinimapCrawlerMoveHelper : BaseCrawlerMoveHelper
    {
        public override ECrawlerMoveOrder HelperKey => ECrawlerMoveOrder.ShowMinimap;


        public override async Awaitable Execute(PartyData party, CrawlerMoveStatus status, CancellationToken token)
        {
            _dispatcher.Dispatch(new ShowPartyMinimap() { Party = party });
            await Task.CompletedTask;
        }
    }
}


