using Assets.Scripts.ClientEvents;
using Assets.Scripts.Crawler.Services.CrawlerMaps;
using Genrpg.Shared.Crawler.Maps.Services;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Services;
using Genrpg.Shared.Entities.Constants;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.Crawler.UI.WorldUI
{
    public class PartyBuffUI : BaseBehaviour, IPointerEnterHandler, IPointerExitHandler
    {

        protected ICrawlerService _crawlerService = null;
        protected ICrawlerWorldService _worldService = null;
        protected ICrawlerMapService _crawlerMapService = null;

        public int PartyBuffId;
        public GameObject ContentRoot;

        public override void Init()
        {
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

        public void OnPointerEnter(PointerEventData eventData)
        {
            _dispatcher.Dispatch(new ShowInfoPanelEvent() { EntityTypeId = EntityTypes.PartyBuff, EntityId = PartyBuffId });
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _dispatcher.Dispatch(new HideInfoPanelEvent());
        }
    }
}
