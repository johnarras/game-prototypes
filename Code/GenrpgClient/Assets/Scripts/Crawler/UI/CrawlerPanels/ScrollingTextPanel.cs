using Assets.Scripts.Crawler.ClientEvents.ActionPanelEvents;
using Assets.Scripts.UI.Abstractions;
using Assets.Scripts.UI.Core;
using Assets.Scripts.UI.Crawler.ActionUI;
using Genrpg.Shared.Crawler.Combat.Constants;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Services;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.UI.Crawler.CrawlerPanels
{
    public class ScrollingTextPanel : BaseBehaviour
    {

        private ICrawlerService _crawlerService = null;

        public GameObject Content;
        public GameObject Parent;

        public GScrollRect ScrollRect;

        public ActionPanelText PanelText;

        public NamedSlider ScrollSpeedSlider;

        private List<object> _subObjects = new List<object>();

        public override void Init()
        {
            _dispatcher.AddListener<AddActionPanelText>(OnAddActionPanelText, GetToken());
            _dispatcher.AddListener<CrawlerStateData>(OnNewStateData, GetToken());

            ScrollSpeedSlider.InitSlider(0, CrawlerCombatConstants.ScrollingFramesValues.Length - 1,
                _crawlerService.GetParty().ScrollFramesIndex, true, OnChangeSlider);


        }

        private void OnChangeSlider(float newValue)
        {
            _crawlerService.GetParty().ScrollFramesIndex = (int)newValue;
        }

        public void OnNewStateData(CrawlerStateData stateData)
        {
            Clear();
        }

        public void Clear()
        {
            _clientEntityService.DestroyAllChildren(Content);
            _subObjects.Clear();
            _clientEntityService.SetActive(Parent, false);
        }

        private void OnAddActionPanelText(AddActionPanelText addText)
        {
            PartyData party = _crawlerService.GetParty();

            if (party.Combat == null)
            {
                return;
            }

            _clientEntityService.SetActive(Parent, true);
            ActionPanelText newText = _clientEntityService.FullInstantiate(PanelText);
            _clientEntityService.AddToParent(newText, Content);
            _subObjects.Add(newText);
            newText.SetText(addText);
            _uiService.ScrollToBottom(ScrollRect);
        }
    }
}
