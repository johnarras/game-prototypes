using Genrpg.Shared.Crawler.Constants;
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
        private ICrawlerService _crawlerService = null;
        private ICrawlerWorldService _worldService = null;


        public GText MapNameText;
        public GText PosLevelText;
        public GText TimeOfDayText;


        private UpdateCrawlerUI _update = null;
        public override void Init()
        {
            AddUpdate(OnLateUpdate, UpdateTypes.Late);
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

            if (map == null || party.HasFlag(PartyFlags.InGuildHall))
            {
                _uiService.SetText(MapNameText, "");
                _uiService.SetText(PosLevelText, "");
            }
            else
            {
                _uiService.SetText(MapNameText, map.GetName(party.CurrPos.X, party.CurrPos.Z));
                _uiService.SetText(PosLevelText, "Lev: " + map.GetMapLevelAtPoint(party.CurrPos.X, party.CurrPos.Z) + " @(" + party.CurrPos.X + "," + party.CurrPos.Z + ")");
            }
        }

        private void OnUIUpdate(UpdateCrawlerUI update)
        {
            _update = update;
        }
    }
}


