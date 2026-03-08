using Assets.Scripts.Crawler.ClientEvents.HUD;
using Genrpg.Shared.Crawler.Currencies.Settings;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Services;
using Genrpg.Shared.DataStores.Indexes;
using Genrpg.Shared.Entities.Constants;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Crawler.UI.Currencies
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

            IReadOnlyList<CrawlerCurrencyType> ctypes = _gameData.Get<CrawlerCurrencySettings>(_gs.ch).GetData();

            _clientEntityService.DestroyAllChildren(IconAnchor);

            PartyData party = _crawlerService.GetParty();

            foreach (CrawlerCurrencyType ctype in ctypes)
            {
                CrawlerCurrencyIcon icon = _clientEntityService.FullInstantiate(IconPrefab);
                _clientEntityService.AddToParent(icon, IconAnchor);
                icon.SetEntityData(EntityTypes.CrawlerCurrency, ctype.IdKey, party.Currencies[ctype.IdKey], 0);
            }
        }

        private void OnResetCrawlerHUD(ResetCrawlerHUD reset)
        {
            CreateIcons();
        }
    }
}


