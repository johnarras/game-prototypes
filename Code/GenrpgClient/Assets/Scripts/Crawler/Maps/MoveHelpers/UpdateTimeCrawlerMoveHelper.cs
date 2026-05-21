using Assets.Scripts.Crawler.Maps.Services.Entities;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.TimeOfDay.Constants;
using OxDb.SharedGame.Crawler.TimeOfDay.Services;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts.Crawler.Maps.MoveHelpers
{
    public class UpdateTimeCrawlerMoveHelper : BaseCrawlerMoveHelper
    {
        public override int Order => 700;

        private ITimeOfDayService _timeService = null;
        public override async Awaitable Execute(PartyData party, CrawlerMoveStatus status, CancellationToken token)
        {
            if (status.MovedPosition)
            {
                await _timeService.UpdateTime(party, ECrawlerTimeUpdateTypes.Move);
            }
        }
    }
}


