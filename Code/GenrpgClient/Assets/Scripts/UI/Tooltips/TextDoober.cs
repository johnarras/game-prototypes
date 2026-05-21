using Assets.Scripts.ClientEvents.UI;
using Assets.Scripts.WorldCanvas.Interfaces;

namespace Assets.Scripts.UI.Tooltips
{
    public class TextDoober : BaseBehaviour, IDynamicUIItem
    {

        protected IClientAppService _appService = null;

        public GText Text;

        protected bool _updateIsComplete = false;

        protected float _startFontSize = 20;

        public override void Init()
        {
            _dispatcher.AddListener<HideTextTooltipEvent>(OnHideTextTooltip, GetToken());
            _updateIsComplete = false;

            _startFontSize = Text.fontSize;
        }

        public void SetText(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                _uiService.SetText(Text, text);
                _updateIsComplete = false;
                Text.fontSize = _startFontSize * _appService.ScreenWidth / 1920;
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


