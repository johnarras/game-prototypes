using OxDb.Client.Crawler.Maps.Services;
using OxDb.Client.Crawler.Services.CrawlerMaps;
using OxDb.Client.Entities.UI;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.States.Services;
using UnityEngine;

namespace OxDb.Client.Crawler.UI.WorldUI
{
    public class PartyBuffUI : EntityIcon
    {

        protected ICrawlerService _crawlerService = null;
        protected ICrawlerWorldService _worldService = null;
        protected ICrawlerMapService _crawlerMapService = null;

        public int PartyBuffId;
        public GameObject ContentRoot;

        public override void Init()
        {
            _entityTypeId = EntityTypes.PartyBuff;
            _entityId = PartyBuffId;
            AddUpdate(FrameUpdate, UpdateTypes.Regular);
        }

        protected virtual void FrameUpdateInternal(PartyData party)
        {

        }

        protected void FrameUpdate()
        {
            PartyData party = _crawlerService.GetParty();

            if (party == null || party.Buffs[PartyBuffId] == 0)
            {
                _clientEntityService.SetActive(ContentRoot, false);
            }
            else
            {
                _clientEntityService.SetActive(ContentRoot, true);
                FrameUpdateInternal(party);
            }

        }
    }
}


