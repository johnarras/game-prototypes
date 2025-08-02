using Assets.Scripts.Crawler.ClientEvents.ActionPanelEvents;

namespace Assets.Scripts.UI.Crawler.ActionUI
{
    public class ActionPanelText : BaseBehaviour
    {
        public GButton Button;
        public GText Text;
        private string _text = null;

        public void SetText(AddActionPanelText text)
        {
            _text = text.Text;
            _uiService.SetText(Text, _text);

            if (Button != null && text.OnClickAction != null)
            {
                _uiService.SetButton(Button, "APT", text.OnClickAction);
            }
        }
    }
}
