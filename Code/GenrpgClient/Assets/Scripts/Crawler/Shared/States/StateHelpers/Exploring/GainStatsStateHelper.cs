using Assets.Scripts.Crawler.Services.CrawlerMaps;
using Genrpg.Shared.Client.GameEvents;
using Genrpg.Shared.Crawler.Maps.Constants;
using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Crawler.States.StateHelpers;
using Genrpg.Shared.Crawler.Stats.Services;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.Stats.Settings.Stats;
using Genrpg.Shared.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Scripts.Crawler.Shared.States.StateHelpers.Exploring
{
    public class GainStatsStateHelper : BaseStateHelper
    {

        private ICrawlerStatService _crawlerStatService = null;
        private ICrawlerMapService _mapService = null;

        public override ECrawlerStates Key => ECrawlerStates.GainStats;

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            PartyData party = _crawlerService.GetParty();

            List<StatType> okStats = _gameData.Get<StatSettings>(_gs.ch).GetData().Where(x => x.IdKey >= StatConstants.PrimaryStatStart && x.IdKey <= StatConstants.PrimaryStatEnd).ToList();

            StringBuilder sb = new StringBuilder();

            sb.Append("You see a vial of");

            int positionHash = _mapService.GetMapCellHash(party.CurrPos.MapId, party.CurrPos.X, party.CurrPos.Z, MapEncounters.Stats);

            StatType statType = okStats[positionHash % okStats.Count];

            if (!string.IsNullOrEmpty(statType.ColorName) && !string.IsNullOrEmpty(statType.ColorCode))
            {
                sb.Append(" " + _textService.HighlightText(statType.ColorName, statType.ColorCode));
            }
            sb.Append(" liquid.\n\n");
            stateData.AddText(sb.ToString());

            stateData.AddText("Who will drink it?\n\n");

            CrawlerMap map = _worldService.GetMap(party.CurrPos.MapId);

            CrawlerMapStatus mapStatus = party.GetMapStatus(party.CurrPos.MapId, true);

            int statAdded = 5 + 5 * (map.Level / 10);

            List<PartyMember> members = party.GetActiveParty();
          
            for (int p = 0; p < members.Count; p++)
            {
                PartyMember pm = members[p];
                stateData.Actions.Add(new CrawlerStateAction(pm.Name, (char)('1' + p), ECrawlerStates.ExploreWorld,
                    () =>
                    {
                        pm.AddPermStat(statType.IdKey, statAdded);
                        mapStatus.OneTimeEncounters.Add(new Genrpg.Shared.Utils.Data.PointXZ(party.CurrPos.X, party.CurrPos.Z));
                        _crawlerStatService.CalcUnitStats(party, pm, false);
                        _mapService.ClearCellObject(party.CurrPos.X, party.CurrPos.Z);
                        _dispatcher.Dispatch(new ShowFloatingText("+ " + statAdded + " " + statType.Name + "!"));
                    }));
            }


            stateData.Actions.Add(new CrawlerStateAction("", CharCodes.Escape, ECrawlerStates.ExploreWorld));


            await Task.CompletedTask;
            return stateData;
        }
    }
}
