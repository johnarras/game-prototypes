using OxDb.Client.Crawler.Maps.EncounterHelpers;
using OxDb.Client.Crawler.Maps.GameObjects;
using OxDb.Client.Crawler.Maps.Services.DrawEntityHelpers;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.Worlds.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace OxDb.Client.Crawler.Maps.Services.DrawCellHelpers
{
    public class EncounterDrawCellHelper : BaseCrawlerDrawCellHelper
    {
        public override ECrawlerDrawCellOrder HelperKey => ECrawlerDrawCellOrder.Encounters;

        public override async ValueTask DrawCell(PartyData party, CrawlerWorld world, CrawlerMapRoot mapRoot, ClientMapCell cell, CancellationToken token)
        {
            long encounterId = _mapService.GetCurrentEncounterAtCell(party, mapRoot.Map, cell.MapX, cell.MapZ, true);

            if (encounterId > 0)
            {
                IClientMapEncounterHelper helper = _mapService.GetEncounterHelper(encounterId);
                if (helper != null)
                {
                    await helper.DrawCell(party, world, mapRoot, cell, cell.MapX, cell.MapZ, token);
                }
            }

            await Task.CompletedTask;
        }
    }
}


