using Assets.Scripts.Awaitables;
using Assets.Scripts.Crawler.ClientEvents.ActionPanelEvents;
using Assets.Scripts.UI.Abstractions;
using Assets.Scripts.UI.Core;
using Assets.Scripts.UI.Crawler.ActionUI;
using Genrpg.Shared.Crawler.Combat.Constants;
using Genrpg.Shared.Crawler.States.Services;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.UI.Crawler.CrawlerPanels
{
    public class ScrollingTextPanel : BaseBehaviour
    {

        private ICrawlerService _crawlerService = null;
        private IAwaitableService _awaitableService = null;

        public GameObject Content;
        public GameObject Parent;

        public GScrollRect ScrollRect;

        public ActionPanelText PanelText;

        public NamedSlider ScrollSpeedSlider;

        private ConcurrentQueue<AddActionPanelText> _textToShow = new ConcurrentQueue<AddActionPanelText>();

        private List<object> _subObjects = new List<object>();

        public override void Init()
        {
            _dispatcher.AddListener<AddActionPanelText>(OnAddActionPanelText, GetToken());
            _dispatcher.AddListener<CrawlerStateData>(OnNewStateData, GetToken());

            ScrollSpeedSlider.InitSlider(0, CrawlerCombatConstants.ScrollingFramesValues.Length - 1,
                _crawlerService.GetParty().ScrollFramesIndex, true, OnChangeSlider);


            AddUpdate(OnLateUpdate, UpdateTypes.Late);

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
            _textToShow.Clear();
            _clientEntityService.DestroyAllChildren(Content);
            _subObjects.Clear();
            _clientEntityService.SetActive(Parent, false);
        }

        private void OnAddActionPanelText(AddActionPanelText addText)
        {
            _textToShow.Enqueue(addText);
        }

        private bool _showingText = false;
        private void OnLateUpdate()
        {

            if (_showingText)
            {
                return;
            }
            if (_textToShow.Count > 0)
            {
                _showingText = true;
                _awaitableService.ForgetAwaitable(OnLateUpdateAsync());
            }

        }

        private async Awaitable OnLateUpdateAsync()
        {
            bool showedText = false;
            while (_textToShow.TryDequeue(out AddActionPanelText action))
            {
                _clientEntityService.SetActive(Parent, true);
                ActionPanelText newText = _clientEntityService.FullInstantiate(PanelText);
                _clientEntityService.AddToParent(newText, Content);
                newText.SetText(action);
                showedText = true;
            }

            if (showedText)
            {
                await Task.Delay(50);
                _uiService.ScrollToBottom(ScrollRect);
            }
            _showingText = false;
        }
    }
}
