using Assets.Scripts.Assets.ObjectPools;
using Assets.Scripts.Crawler.ClientEvents.ActionPanelEvents;
using Assets.Scripts.UI.Abstractions;
using Assets.Scripts.UI.Core;
using Assets.Scripts.UI.Crawler.ActionUI;
using Genrpg.Shared.Client.Assets.Constants;
using Genrpg.Shared.Crawler.Combat.Constants;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Services;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts.UI.Crawler.CrawlerPanels
{
    public class ScrollingTextPanel : BaseBehaviour
    {

        private ICrawlerService _crawlerService = null;
        private IObjectPool _objectPool = null;

        public GameObject Content;
        public GameObject Parent;

        public GScrollRect ScrollRect;

        public ActionPanelText PanelText;

        public const string RowPrefabName = "ActionPanelText";

        public NamedSlider ScrollSpeedSlider;

        private List<ActionPanelText> _subObjects = new List<ActionPanelText>();

        public override void Init()
        {
            _dispatcher.AddListener<AddActionPanelText>(OnAddActionPanelText, GetToken());
            _dispatcher.AddListener<CrawlerStateData>(OnNewStateData, GetToken());

            ScrollSpeedSlider.InitSlider(1, CrawlerCombatConstants.ScrollingFramesValues.Length,
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
            foreach (ActionPanelText apt in _subObjects)
            {
                _objectPool.ReturnObject(apt);
            }
            _subObjects.Clear();
            _clientEntityService.SetActive(Parent, false);
        }

        protected override void OnDestroy()
        {
            Clear();
            base.OnDestroy();
        }

        private void OnAddActionPanelText(AddActionPanelText addText)
        {
            PartyData party = _crawlerService.GetParty();

            if (party.Combat == null)
            {
                return;
            }

            _clientEntityService.SetActive(Parent, true);

            _objectPool.CheckoutObject(Content, AssetCategoryNames.UI, RowPrefabName, OnLoadRow, addText, GetToken(), "CrawlerAction");
        }

        private void OnLoadRow(object obj, object data, CancellationToken token)
        {
            GameObject go = obj as GameObject;
            if (go == null)
            {
                return;
            }

            ActionPanelText apt = go.GetComponent<ActionPanelText>();
            if (apt == null)
            {
                _clientEntityService.Destroy(apt);
                return;
            }

            AddActionPanelText addText = data as AddActionPanelText;

            if (addText == null)
            {
                _clientEntityService.Destroy(go);
                return;
            }

            apt.SetText(addText);
            _subObjects.Add(apt);
            _uiService.ScrollToBottom(ScrollRect);
        }
    }
}
