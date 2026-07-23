using OxDb.Client.Crawler.Maps.Services.Entities;
using OxDb.SharedGame.Crawler.GameEvents;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace OxDb.Client.Crawler.Maps.MoveHelpers
{
    public class UpdateUICrawlerMoveHelper : BaseCrawlerMoveHelper
    {
        public override ECrawlerMoveOrder HelperKey => ECrawlerMoveOrder.UpdateUI;


        public override async Awaitable Execute(PartyData party, CrawlerMoveStatus status, CancellationToken token)
        {
            await Task.CompletedTask;
            _dispatcher.Dispatch(new UpdateCrawlerUI());
        }
    }
}


