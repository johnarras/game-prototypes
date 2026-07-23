using OxDb.Client.ClientEvents;
using OxDb.Client.UI.Crawler.CrawlerPanels;
using OxDb.Client.UI.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Crawler.States.Constants;
using OxDb.SharedGame.Crawler.States.Entities;
using OxDb.SharedGame.Crawler.States.Services;
using UnityEngine;
using UnityEngine.InputSystem;

namespace OxDb.Client.UI.Crawler.ActionUI
{
    public class ActionPanelRow : BaseBehaviour
    {

        protected IInputService _inputService = null;

        public GText Text;
        public GButton Button;
        private ICrawlerService _crawlerService = null;
        private ITextService _textService = null;

        protected CrawlerStateAction _action = null;
        protected CrawlerStateData _state = null;

        public void SetAction(CrawlerStateWithAction fullAction)
        {
            _action = fullAction.Action;
            _state = fullAction.State;

            _uiService.AddPointerHandlers(gameObject, OnPointerEnter, OnPointerExit);

            if (_action != null)
            {
                string text = _action.Text;

                if (_action.Key == Key.Escape)
                {
                    text = $"\n\nPress {_textService.HighlightText("Escape")} to return to " + StrUtils.SplitOnCapitalLetters(_action.NextState.ToString());
                }
                else if (text != null && text.Length > 0 && char.IsLetterOrDigit(text[0]))
                {
                    if (_inputService.FromChar(text[0]) == _action.Key)
                    {
                        char firstLetter = text[0];
                        text = $"{_textService.HighlightText(text[0])}{text.Substring(1)}";
                    }
                }

                if (_state.UseSmallerButtons)
                {
                    _uiService.SetAutoSizing(Text, true);
                }

                _uiService.SetText(Text, text);

                _uiService.SetButton(Button, "ActionTextRow", ClickAction);

            }
        }

        private void ClickAction()
        {
            if (_action != null && _action.NextState != ECrawlerStates.None)
            {
                _crawlerService.ChangeState(_state, _action, GetToken());
            }
        }

        public void OnPointerExit(GameObject go)
        {
            _uiService.SetAlpha(Text, 1.0f);
            if (_action != null && _action.OnPointerExit != null)
            {
                _action?.OnPointerExit(go);
            }
            else
            {
                _dispatcher.Dispatch(new HideInfoPanelEvent());
            }
        }

        public void OnPointerEnter(GameObject go)
        {

            if (_action.NextState != ECrawlerStates.None || _action.OnClickAction != null)
            {
                _uiService.SetAlpha(Text, 0.7f);
            }

            if (_action != null)
            {
                if (!string.IsNullOrEmpty(_action.SpriteName))
                {
                    _dispatcher.Dispatch<ShowWorldPanelImage>(new ShowWorldPanelImage(_action.SpriteName));
                }
                if (_action.OnPointerEnter != null)
                {
                    _action.OnPointerEnter(go);
                }
            }
        }

        public override void OnReturn()
        {
            base.OnReturn();
            _uiService.SetText(Text, null);
            _uiService.ClearButton(Button);
            _uiService.SetAlpha(Text, 1.0f);
        }
    }
}


