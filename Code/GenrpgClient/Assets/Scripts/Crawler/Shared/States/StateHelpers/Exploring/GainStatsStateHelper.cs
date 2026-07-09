using Assets.Scripts.Crawler.Services.CrawlerMaps;
using Assets.Scripts.FloatingText.ClientEvents;
using OxDb.SharedGame.Crawler.Maps.Constants;
using OxDb.SharedGame.Crawler.Maps.Entities;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.States.Constants;
using OxDb.SharedGame.Crawler.States.Entities;
using OxDb.SharedGame.Crawler.States.StateHelpers;
using OxDb.SharedGame.Crawler.Stats.Services;
using OxDb.SharedGame.Stats.Constants;
using OxDb.SharedGame.Stats.Settings.Stats;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.InputSystem;

namespace Assets.Scripts.Crawler.Shared.States.StateHelpers.Exploring
{
    public class GainStatsStateHelper : BaseStateHelper
    {

        private ICrawlerStatService _crawlerStatService = null;
        private ICrawlerMapService _mapService = null;

        public override ECrawlerStates HelperKey => ECrawlerStates.GainStats;

        public override async ValueTask<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
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

            long statAdded = 5 + 5 * (map.Level / 10);

            List<PartyMember> members = party.ActiveParty;

            for (int p = 0; p < members.Count; p++)
            {
                PartyMember pm = members[p];
                stateData.Actions.Add(new CrawlerStateAction(pm.Name, FromChar((char)('1' + p)), ECrawlerStates.ExploreWorld,
                    () =>
                    {
                        pm.AddPermStat(statType.IdKey, statAdded);
                        int index = map.GetIndex(party.CurrPos.X, party.CurrPos.Z);
                        mapStatus.Encounters.SetBitIndex(index);
                        _crawlerStatService.CalcUnitStats(party, pm, false);
                        _mapService.ClearCellObject(party.CurrPos.X, party.CurrPos.Z);
                        _dispatcher.Dispatch(new ShowFloatingText("+ " + statAdded + " " + statType.Name + "!"));
                    }));
            }


            stateData.Actions.Add(new CrawlerStateAction("", Key.Escape, ECrawlerStates.ExploreWorld));


            await Task.CompletedTask;
            return stateData;
        }
    }
}


