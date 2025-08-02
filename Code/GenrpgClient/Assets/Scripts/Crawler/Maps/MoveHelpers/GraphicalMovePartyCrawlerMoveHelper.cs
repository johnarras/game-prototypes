using Assets.Scripts.Crawler.Maps.Services.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Crawler.Maps.MoveHelpers
{

    public class GraphicalMovePartyCrawlerMoveHelper : BaseCrawlerMoveHelper
    {
        public override int Order => 600;

        public override async Awaitable Execute(PartyData party, CrawlerMoveStatus status, CancellationToken token)
        {
            _mapService.MovePartyTo(party, party.CurrPos.X, party.CurrPos.Z, party.CurrPos.Rot, false, token);
            await Task.CompletedTask;
        }
    }
}
