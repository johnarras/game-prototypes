using Assets.Scripts.Crawler.Services.CrawlerMaps;
using Assets.Scripts.Entities.UI;
using Genrpg.Shared.Crawler.Maps.Services;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Services;
using Genrpg.Shared.Entities.Constants;
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
            _updateService.AddUpdate(this, FrameUpdate, UpdateTypes.Regular, GetToken());
        }

        protected virtual void FrameUpdateInternal(PartyData party)
        {

        }

        protected void FrameUpdate()
        {
            PartyData party = _crawlerService.GetParty();

            if (party == null || party.Buffs.Get(PartyBuffId) == 0)
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
