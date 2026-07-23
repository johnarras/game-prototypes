using OxDb.SharedCore.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace OxDb.Client.Assets.Textures
{

    [Serializable]
    public class LerpState
    {
        public Color Color;
        public float Time = 0.3f;
        public Vector3 AnchorOffset;
    }


    public class ColorLerp : BaseBehaviour
    {

        private IClientAppService _clientAppService = null;

        public GImage LerpImage;

        public List<GameObject> LerpAnchors = new List<GameObject>();

        private int _lerpIndex = 0;
        private bool _lerpingNow = false;
        private float _elapsedTime = 0;
        public List<LerpState> LerpStates = new List<LerpState>();

        public override void Init()
        {
            _updateService.AddUpdate(this, UpdateColorLerp, UpdateTypes.Regular, GetToken());
        }


        public void SetLerpingNow(bool lerpingNow)
        {
            if (_lerpingNow == lerpingNow)
            {
                return;
            }

            _lerpingNow = lerpingNow;
            SetLerpIndex(0);
        }

        private void SetLerpIndex(int index)
        {
            _lerpIndex = index;
            _elapsedTime = 0;

            if (index >= 0 && index < LerpStates.Count)
            {
                SetLerpColor(LerpStates[0].Color);
                SetAnchorOffset(LerpStates[0].AnchorOffset);
            }
        }

        private void SetAnchorOffset(Vector3 pos)
        {

            for (int a = 0; a < LerpAnchors.Count; a++)
            {
                if (LerpAnchors[a] != null)
                {
                    LerpAnchors[a].transform.localPosition = pos;
                }
            }
        }

        private void SetLerpColor(Color color)
        {
            if (LerpImage != null)
            {
                LerpImage.SetColor(color);
            }
        }

        private void UpdateColorLerp()
        {
            if (!_lerpingNow || _lerpIndex < 0 || LerpStates.Count < 1)
            {
                return;
            }

            _elapsedTime += _clientAppService.GetDeltaTime();

            LerpState currState = LerpStates[_lerpIndex % LerpStates.Count];



            if (_elapsedTime >= currState.Time)
            {
                _elapsedTime -= currState.Time;
                _lerpIndex++;
                if (_lerpIndex >= LerpStates.Count)
                {
                    _lerpIndex = 0;
                }
                currState = LerpStates[_lerpIndex % LerpStates.Count];
            }

            _elapsedTime = MathUtil.Clamp(0, _elapsedTime, currState.Time);

            float pct = 1;

            if (currState.Time > 0)
            {
                pct = MathUtil.Clamp(0, _elapsedTime / currState.Time, 1);
            }

            pct = Mathf.SmoothStep(0, 1, pct);
            LerpState nextState = LerpStates[(_lerpIndex + 1) % LerpStates.Count];

            SetLerpColor(Color.Lerp(currState.Color, nextState.Color, pct));
            SetAnchorOffset(Vector3.Lerp(currState.AnchorOffset, nextState.AnchorOffset, pct));

        }
    }
}
