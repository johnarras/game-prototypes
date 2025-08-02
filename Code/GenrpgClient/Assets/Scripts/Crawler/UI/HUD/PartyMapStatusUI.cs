using Assets.Scripts.Crawler.Services.CrawlerMaps;
using Genrpg.Shared.Crawler.GameEvents;
using Genrpg.Shared.Crawler.Maps.Constants;
using Genrpg.Shared.Crawler.Maps.Services;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Services;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Crawler.UI.HUD
{
    public class PartyMapStatusUI : BaseBehaviour
    {

        private ICrawlerService _crawlerService;
        private ICrawlerMapService _crawlerMapService;


        public GImage NoMagicImage;
        public GImage PeacefulImage;


        public override void Init()
        {
            base.Init();
            _dispatcher.AddListener<UpdateCrawlerUI>(OnUpdateWorldUI, GetToken());
            _updateService.AddUpdate(gameObject, OnLateUpdate, UpdateTypes.Late, GetToken());
        }


        private UpdateCrawlerUI _update = null;
        private void OnUpdateWorldUI(UpdateCrawlerUI update)
        {
            _update = update;
        }

        private void OnLateUpdate()
        {
            if (_update == null)
            {
                return;
            }
            _update = null;

            PartyData party = _crawlerService.GetParty();
            int magicBits = _crawlerMapService.GetMagicBits(party.CurrPos.MapId, party.CurrPos.X, party.CurrPos.Z);

            _clientEntityService.SetActive(PeacefulImage, FlagUtils.IsSet(magicBits, MapMagics.Peaceful));
            _clientEntityService.SetActive(NoMagicImage, FlagUtils.IsSet(magicBits, MapMagics.NoMagic));

        }
    }
}
