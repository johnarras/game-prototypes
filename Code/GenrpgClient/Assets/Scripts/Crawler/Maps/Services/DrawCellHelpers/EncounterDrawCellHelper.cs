using Assets.Scripts.Crawler.Maps.EncounterHelpers;
using Assets.Scripts.Crawler.Maps.GameObjects;
using Assets.Scripts.Crawler.Maps.Services.DrawEntityHelpers;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.Worlds.Entities;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Crawler.Maps.Services.DrawCellHelpers
{
    public class EncounterDrawCellHelper : BaseCrawlerDrawCellHelper
    {
        public override int Order => 500;

        public override async Awaitable DrawCell(PartyData party, CrawlerWorld world, CrawlerMapRoot mapRoot, ClientMapCell cell, int xpos, int zpos, int realCellX, int realCellZ, CancellationToken token)
        {
            long encounterId = _mapService.GetCurrentEncounterAtCell(party, mapRoot.Map, cell.MapX, cell.MapZ, true);

            if (encounterId > 0)
            {
                IClientMapEncounterHelper helper = _mapService.GetEncounterHelper(encounterId);
                if (helper != null)
                {
                    await helper.DrawCell(party, world, mapRoot, cell, realCellX, realCellZ, token);
                }
            }

            await Task.CompletedTask;
        }
    }
}


