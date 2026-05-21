using Assets.Scripts.Crawler.Services.CrawlerMaps;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Crawler.GameEvents;
using OxDb.SharedGame.Crawler.Maps.Constants;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.States.Services;

namespace Assets.Scripts.Crawler.UI.HUD
{
    public class MapCellStatusUI : BaseBehaviour
    {

        private ICrawlerService _crawlerService = null;
        private ICrawlerMapService _crawlerMapService = null;


        public GImage NoMagicImage;
        public GImage SilenceImage;


        public override void Init()
        {
            base.Init();
            _dispatcher.AddListener<UpdateCrawlerUI>(OnUpdateWorldUI, GetToken());
            AddUpdate(OnLateUpdate, UpdateTypes.Late);
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
            int magicBits = _crawlerMapService.GetMagicBits(party.CurrPos.MapId, party.CurrPos.X, party.CurrPos.Z, true);

            _clientEntityService.SetActive(SilenceImage, FlagUtils.MatchesAnyBits(magicBits, MapMagics.Silence));
            _clientEntityService.SetActive(NoMagicImage, FlagUtils.MatchesAnyBits(magicBits, MapMagics.NoMagic));

        }
    }
}


