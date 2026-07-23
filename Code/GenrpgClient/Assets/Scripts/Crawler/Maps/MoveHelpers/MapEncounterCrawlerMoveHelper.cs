using OxDb.Client.Crawler.Maps.EncounterHelpers;
using OxDb.Client.Crawler.Maps.Services.Entities;
using OxDb.SharedGame.Crawler.Maps.Entities;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using System.Threading;
using UnityEngine;

namespace OxDb.Client.Crawler.Maps.MoveHelpers
{
    public class MapEncounterCrawlerMoveHelper : BaseCrawlerMoveHelper
    {
        public override ECrawlerMoveOrder HelperKey => ECrawlerMoveOrder.MapEncounters;



        public override async Awaitable Execute(PartyData party, CrawlerMoveStatus moveStatus, CancellationToken token)
        {
            if (moveStatus.MoveIsStopped || !moveStatus.MovedPosition)
            {
                return;
            }

            CrawlerMap map = moveStatus.MapRoot.Map;

            long encounterTypeId = _mapService.GetCurrentEncounterAtCell(party, map, party.CurrPos.X, party.CurrPos.Z, true);

            IClientMapEncounterHelper encounterHelper = _mapService.GetEncounterHelper(encounterTypeId);

            CrawlerMapStatus mapStatus = party.GetMapStatus(map.IdKey, true);

            if (encounterHelper != null)
            {
                await encounterHelper.OnEnterCell(party, map, mapStatus, moveStatus, token);
            }
        }
    }
}


