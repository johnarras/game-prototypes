using Assets.Scripts.Crawler.Maps.EncounterHelpers;
using Assets.Scripts.Crawler.Maps.GameObjects;
using Assets.Scripts.Crawler.Maps.Services.DrawEntityHelpers;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Worlds.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            long encounterId = _mapService.GetEncounterAtCell(party, mapRoot.Map, cell.MapX, cell.MapZ);

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
