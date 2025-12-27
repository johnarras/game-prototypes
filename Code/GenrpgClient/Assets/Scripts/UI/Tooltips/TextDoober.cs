using Assets.Scripts.ClientEvents.UI;
using Assets.Scripts.WorldCanvas.Interfaces;

namespace Assets.Scripts.UI.Tooltips
{
    public class TextDoober : BaseBehaviour, IDynamicUIItem
    {
        public GText Text;

        protected bool _updateIsComplete = false;

        public override void Init()
        {
            _dispatcher.AddListener<HideTextTooltipEvent>(OnHideTextTooltip, GetToken());
            _updateIsComplete = false;
        }

        public void SetText(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                _uiService.SetText(Text, text);
                _updateIsComplete = false;
            }
            else
            {
                _updateIsComplete = true;
            }

        }


        protected void OnHideTextTooltip(HideTextTooltipEvent hideText)
        {
            _updateIsComplete = true;
        }


        public bool FrameUpdateIsComplete(float deltaTime)
        {
            return _updateIsComplete;
        }
    }
}


