using Assets.Scripts.UI.Interfaces;
using Genrpg.Shared.Crawler.Info.Constants;
using Genrpg.Shared.Crawler.Info.Services;

namespace Assets.Scripts.Info.UI
{
    public class InfoPanelRow : BaseBehaviour
    {

        private IInfoService _infoService;
        private ITextService _textService;
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

        public void InitData(InfoPanel panel, string text)
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
                _uiService.SetButton(Button, GetType().Name, OnClickText);
            }
        }

        protected virtual void OnClickText()
        {
            if (string.IsNullOrEmpty(_text))
            {
                return;
            }

            _panel.ShowLines(_infoService.GetInfoLines(_textService.GetLinkUnderMouse(Text)));

        }
    }
}
