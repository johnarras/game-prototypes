using Assets.Scripts.Crawler.Maps.Services.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.TimeOfDay.Constants;
using Genrpg.Shared.Crawler.TimeOfDay.Services;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts.Crawler.Maps.MoveHelpers
{
    public class UpdateTimeCrawlerMoveHelper : BaseCrawlerMoveHelper
    {
        public override int Order => 700;

        private ITimeOfDayService _timeService;
        public override async Awaitable Execute(PartyData party, CrawlerMoveStatus status, CancellationToken token)
        {
            await _timeService.UpdateTime(party, ECrawlerTimeUpdateTypes.Move);
        }
    }
}
