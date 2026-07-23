using OxDb.Client.Crawler.ClientEvents.HUD;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.States.Services;
using OxDb.SharedGame.Currencies.Constants;
using OxDb.SharedGame.Currencies.Settings;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OxDb.Client.Crawler.UI.Currencies
{
    public class CrawlerCurrencyUI : BaseBehaviour
    {
        protected ICrawlerService _crawlerService = null;
        public GameObject IconAnchor;
        public CrawlerCurrencyIcon IconPrefab;

        public override void Init()
        {
            _dispatcher.AddListener<ResetCrawlerHUD>(OnResetCrawlerHUD, GetToken());
            CreateIcons();
        }


        private void CreateIcons()
        {

            IReadOnlyList<CoreCurrencyType> ctypes = _gameData.Get<CoreCurrencyTypeSettings>(_gs.ch).GetData();

            _clientEntityService.DestroyAllChildren(IconAnchor);

            PartyData party = _crawlerService.GetParty();

            ctypes = ctypes.Where(x => x.IdKey == CoreCurrencyTypes.Coins || x.StatTypeId > 0).ToList();

            foreach (CoreCurrencyType ctype in ctypes)
            {
                CrawlerCurrencyIcon icon = _clientEntityService.FullInstantiate(IconPrefab);
                _clientEntityService.AddToParent(icon, IconAnchor);
                icon.SetEntityData(EntityTypes.CoreCurrency, ctype.IdKey, party.Currencies[ctype.IdKey], 0);
            }
        }

        private void OnResetCrawlerHUD(ResetCrawlerHUD reset)
        {
            CreateIcons();
        }
    }
}


