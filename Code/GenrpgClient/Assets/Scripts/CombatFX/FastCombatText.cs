using Assets.Scripts.WorldCanvas.Interfaces;
using UnityEngine;

namespace Assets.Scripts.UI.CombatTexts
{
    public class FastCombatText : BaseBehaviour, IDynamicUIItem
    {

        private IClientAppService _appService = null;

        public GText Text;

        private int _framePerSecond = 30;

        private float _elapsedSeconds = 0;

        private FastCombatTextArgs _args = null;

        public void Init(FastCombatTextArgs args)
        {
            _args = args;

            if (_args == null)
            {
                return;
            }
            _elapsedSeconds = 0;
            _framePerSecond = _appService.TargetFrameRate;

            _uiService.SetText(Text, args.Text);
            _uiService.SetColor(Text, args.Color);

        }

        public bool FrameUpdateIsComplete(float deltaTime)
        {
            if (_args == null)
            {
                return true;
            }

            _elapsedSeconds += 1.0f / _framePerSecond;

            transform.localPosition += new Vector3(_args.FrameDx, _args.FrameDy, 0);

            if (_elapsedSeconds >= _args.AnimateTime)
            {
                return true;
            }

            return false;
        }
    }
}
