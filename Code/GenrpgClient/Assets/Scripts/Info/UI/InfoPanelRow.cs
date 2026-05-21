using Assets.Scripts.UI.Interfaces;
using OxDb.SharedGame.Crawler.Info.Constants;
using OxDb.SharedGame.Crawler.Info.Services;

namespace Assets.Scripts.Info.UI
{
    public class InfoPanelRow : BaseBehaviour
    {

        private IInfoService _infoService = null;
        private ITextService _textService = null;
        public GText Text;
        public GButton Button;

        private string _text;

        private InfoPanel _panel;


        public override void OnReturn()
        {
            base.OnReturn();
            _uiService.SetText(Text, null);
            _uiService.ClearButton(Button);
        }

        public void SetData(InfoPanel panel, string text)
        {
            _panel = panel;
            _text = text;
            _uiService.SetText(Text, _text);

            if (string.IsNullOrEmpty(_text))
            {
                return;
            }

            if (_text.IndexOf(InfoConstants.LinkPrefix) > 0 && _text.LastIndexOf(InfoConstants.LinkMiddle) > _text.IndexOf(InfoConstants.LinkPrefix))
            {
                _uiService.SetButton(Button, name, OnClickText);
            }
        }

        protected virtual void OnClickText()
        {
            if (string.IsNullOrEmpty(_text))
            {
                return;
            }

            _panel.ShowLines(_infoService.GetInfoPanelArgs(_textService.GetLinkUnderMouse(Text)), EInfoPanelDisplayReason.Click);

        }
    }
}


