using Assets.Scripts.ClientEvents;
using Assets.Scripts.UI.Interfaces;
using Genrpg.Shared.Crawler.Info.Services;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.Crawler.UI.WorldUI
{
    public class RolloverInfoRow : BaseBehaviour, IPointerMoveHandler
    {
        public GText MainText;
        protected ITextService _textService;
        protected IInfoService _infoService;

        protected string _currentLink = null;
        public override void Init()
        {
            _uiService.AddPointerHandlers(MainText, OnPointerEnter, OnPointerExit);
        }


        public virtual void OnPointerExit(GameObject go)
        {
            _currentLink = null;
            _dispatcher.Dispatch(new HideInfoPanelEvent());
        }

        public virtual void OnPointerEnter(GameObject go)
        {
            UpdateLinkShown();
        }

        public void OnPointerMove(PointerEventData eventData)
        {
            UpdateLinkShown();
        }

        private void UpdateLinkShown()
        {
            string linkText = _textService.GetLinkUnderMouse(MainText);
            if (linkText != _currentLink)
            {
                List<string> lines = _infoService.GetInfoLines(linkText);

                if (lines.Count > 0)
                {
                    _dispatcher.Dispatch(new ShowInfoPanelEvent() { Lines = lines });
                    _currentLink = linkText;
                }
            }
        }
    }
}
