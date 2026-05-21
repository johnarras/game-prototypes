using Assets.Scripts.Crawler.Services.CrawlerMaps;
using Assets.Scripts.Entities.UI;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedGame.Crawler.Maps.Services;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.States.Services;
using UnityEngine;

namespace Assets.Scripts.Crawler.UI.WorldUI
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


