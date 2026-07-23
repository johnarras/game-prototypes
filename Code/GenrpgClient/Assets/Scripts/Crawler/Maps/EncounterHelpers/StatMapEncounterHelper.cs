using OxDb.Client.Crawler.Maps.GameObjects;
using OxDb.Client.Crawler.Maps.Loading;
using OxDb.Client.Crawler.Maps.Services.Entities;
using OxDb.SharedGame.Crawler.Maps.Constants;
using OxDb.SharedGame.Crawler.Maps.Entities;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.States.Constants;
using OxDb.SharedGame.Crawler.Worlds.Entities;
using OxDb.SharedGame.Stats.Constants;
using OxDb.SharedGame.Stats.Settings.Stats;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OxDb.Client.Crawler.Maps.EncounterHelpers
{
    public class StatMapEncounterHelper : BaseClientMapEncounterHelper
    {
        public override long HelperKey => MapEncounters.Stats;

        public override async ValueTask DrawCell(PartyData party, CrawlerWorld world, CrawlerMapRoot mapRoot, ClientMapCell cell, int x, int z, CancellationToken token)
        {

            List<StatType> okStats = _gameData.Get<StatSettings>(_gs.ch).GetData().Where(x => x.IdKey >= StatConstants.PrimaryStatStart && x.IdKey <= StatConstants.PrimaryStatEnd).ToList();

            StringBuilder sb = new StringBuilder();

            int positionHash = _mapService.GetMapCellHash(mapRoot.Map.IdKey, x, z, MapEncounters.Stats);

            StatType statType = okStats[positionHash % okStats.Count];

            CrawlerObjectLoadData loadData = new CrawlerObjectLoadData()
            {
                MapRoot = mapRoot,
                Cell = cell,
                Data = statType,
            };

            _mapService.LoadProp(loadData, "StatCauldron", token);
            await Task.CompletedTask;
        }

        public override async ValueTask OnEnterCell(PartyData party, CrawlerMap map, CrawlerMapStatus mapStatus, CrawlerMoveStatus moveStatus, CancellationToken token)
        {
            _crawlerService.ChangeState(ECrawlerStates.GainStats, token);
            moveStatus.MoveIsStopped = true;
            await Task.CompletedTask;
        }
    }
}