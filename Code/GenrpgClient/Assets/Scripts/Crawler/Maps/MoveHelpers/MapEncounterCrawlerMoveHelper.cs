using Assets.Scripts.Crawler.Maps.EncounterHelpers;
using Assets.Scripts.Crawler.Maps.Services.Entities;
using OxDb.SharedGame.Crawler.Maps.Entities;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts.Crawler.Maps.MoveHelpers
{
    public class MapEncounterCrawlerMoveHelper : BaseCrawlerMoveHelper
    {
        public override int Order => 400;


        public override async Awaitable Execute(PartyData party, CrawlerMoveStatus moveStatus, CancellationToken token)
        {
            if (moveStatus.MoveIsComplete || !moveStatus.MovedPosition)
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


