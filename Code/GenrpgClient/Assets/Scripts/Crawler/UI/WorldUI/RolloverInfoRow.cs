using Assets.Scripts.ClientEvents;
using Assets.Scripts.UI.Interfaces;
using OxDb.SharedGame.Crawler.Info.Services;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.Crawler.UI.WorldUI
{
    public class RolloverInfoRow : BaseBehaviour, IPointerMoveHandler
    {
        public GText MainText;
        protected ITextService _textService = null;
        protected IInfoService _infoService = null;

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
                ShowInfoPanelArgs args = _infoService.GetInfoPanelArgs(linkText);

                if (args.Lines.Count > 0)
                {
                    _dispatcher.Dispatch(args);
                    _currentLink = linkText;
                }
            }
        }
    }
}


