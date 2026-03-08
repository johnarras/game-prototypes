using Assets.Scripts.Doobers.Events;
using Assets.Scripts.DynamicUI.Services;
using Assets.Scripts.Entities.UI;
using Assets.Scripts.WorldCanvas.Interfaces;
using Genrpg.Shared.Utils;
using UnityEngine;

namespace Assets.Scripts.Doobers.UI
{
    public class Doober : EntityIcon, IDynamicUIItem
    {

        private IDynamicUIService _dynamicUIService = null;

        private Vector3 _startPos;
        private Vector3 _endPos;
        private Vector3 _offsetPos;

        RectTransform _rectTransform;

        private DooberArgs _dooberArgs = null;

        private float _offsetAngle = 0;

        private float _elapsedTime = 0;

        protected override bool IsDooberTarget => false;
        public void SetData(long entityTypeId, long entityId, long quantity, DooberArgs dooberArgs)
        {
            SetEntityData(entityTypeId, entityId, quantity, quantity);
            InitDooberArgs(dooberArgs);
        }

        public void SetData(string atlasName, string spriteName, DooberArgs dooberArgs)
        {
            _spriteService.LoadAtlasSpriteInto(atlasName, spriteName, Icon, GetToken());
            InitDooberArgs(dooberArgs);
        }

        private void InitDooberArgs(DooberArgs dooberArgs)
        {
            _elapsedTime = 0;
            _offsetAngle = 0;
            _rectTransform = GetComponent<RectTransform>();
            _dooberArgs = dooberArgs;
            _startPos = dooberArgs.StartPosition;
            _endPos = dooberArgs.EndPosition;
            if (dooberArgs.SizeScale != 0)
            {
                transform.localScale = Vector3.one * (float)dooberArgs.SizeScale;
            }
            _rectTransform.position = _startPos;
            PointAtEndPosition();
        }

        public bool FrameUpdateIsComplete(float deltaTime)
        {
            if (_dooberArgs == null || _dooberArgs.LerpTime <= 0)
            {
                if (_dooberArgs != null)
                {
                    _dynamicUIService.ReturnDooberArgs(_dooberArgs);
                    _dooberArgs = null;
                }
                return true;
            }
            if (_elapsedTime == 0)
            {
                _offsetAngle = MathUtil.FloatRange(0, 360, _rand);
            }
            _elapsedTime += deltaTime;

            float percentDone = MathUtil.Clamp(0, _elapsedTime / _dooberArgs.LerpTime, 1);

            if (_dooberArgs.StartOffsetSize > 0)
            {
                float sin = Mathf.Sin(_offsetAngle);
                float cos = Mathf.Cos(_offsetAngle);

                _offsetPos = new Vector3(cos, sin, 0) * _dooberArgs.StartOffsetSize * (1 - percentDone) * (1-percentDone) * (percentDone);
            }

            if (_dooberArgs.PercentDonePowerMult > 0)
            {
                percentDone *= Mathf.Pow(percentDone, _dooberArgs.PercentDonePowerMult);
            }

            percentDone = MathUtil.Clamp(0, percentDone, 1);

            _rectTransform.position = Vector2.Lerp(_startPos + _offsetPos, _endPos, percentDone);

            if (percentDone >= 1)
            {
                if (_dooberArgs != null)
                {
                    _dynamicUIService.ReturnDooberArgs(_dooberArgs);
                    _dooberArgs = null; 
                }
                _dynamicUIService.AddEntityQuantityVisual(_entityTypeId, _entityId, _currQuantity, false);
                return true;
            }

            PointAtEndPosition();

            return false;
        }

        private void PointAtEndPosition()
        {

            if (_dooberArgs.PointAtEnd)
            {
                Vector2 posDiff = _endPos - _rectTransform.position;

                float angle = Mathf.Atan2(posDiff.y, posDiff.x) * 180 / Mathf.PI;

                _rectTransform.localEulerAngles = new Vector3(0, 0, angle);
            }
        }
    }
}


