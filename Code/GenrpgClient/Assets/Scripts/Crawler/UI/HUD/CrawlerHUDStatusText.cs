using Genrpg.Shared.Crawler.GameEvents;
using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Maps.Services;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Services;
using System;

namespace Assets.Scripts.Crawler.UI.HUD
{
    public class CrawlerHUDStatusText : BaseBehaviour
    {
        private ICrawlerService _crawlerService;
        private ICrawlerWorldService _worldService;


        public GText MapNameText;
        public GText LevelText;
        public GText MapPositionText;
        public GText TimeOfDayText;
        public GText CompleteText;


        private UpdateCrawlerUI _update = null;
        public override void Init()
        {
            _updateService.AddUpdate(this, OnLateUpdate, UpdateTypes.Late, GetToken());
            _dispatcher.AddListener<UpdateCrawlerUI>(OnUIUpdate, GetToken());
        }

        private void OnLateUpdate()
        {
            if (_update == null)
            {
                return;
            }

            _update = null;
            PartyData party = _crawlerService.GetParty();

            TimeSpan ts = TimeSpan.FromHours(party.HourOfDay);

            _uiService.SetText(TimeOfDayText, ts.ToString(@"hh\:mm") + " Day " + (party.DaysPlayed + 1));

            CrawlerMap map = _worldService.GetMap(party.CurrPos.MapId);

            if (map == null)
            {
                return;
            }

            _uiService.SetText(MapPositionText, "(" + party.CurrPos.X + "," + party.CurrPos.Z + ")");

            _uiService.SetText(MapNameText, map.GetName(party.CurrPos.X, party.CurrPos.Z));
            _uiService.SetText(LevelText, "Level: " + map.GetMapLevelAtPoint(party.CurrPos.X, party.CurrPos.Z));
            _uiService.SetText(CompleteText, party.CompletedMaps.HasBit(map.IdKey) ? "Complete!" : "");
        }

        private void OnUIUpdate(UpdateCrawlerUI update)
        {
            _update = update;
        }
    }
}
