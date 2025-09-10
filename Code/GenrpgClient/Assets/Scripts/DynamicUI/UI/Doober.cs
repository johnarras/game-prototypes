using Assets.Scripts.ClientEvents.UserCoins;
using Assets.Scripts.Doobers.Events;
using Assets.Scripts.WorldCanvas.Interfaces;
using Genrpg.Shared.Utils;
using UnityEngine;

namespace Assets.Scripts.Doobers.UI
{
    public class Doober : BaseBehaviour, IDynamicUIItem
    {
        public GImage Image;

        private long _entityId = 0;
        private long _quantity = 0;
        private Vector3 _startPos;
        private Vector3 _endPos;

        RectTransform _rectTransform;

        private ShowDooberEvent _showDoober = null;

        private float _offsetAngle = 0;

        private float _elapsedTime = 0;

        public void InitData(long entityTypeId, long entityId, long quantity, ShowDooberEvent showDoober)
        {
            _entityId = entityId;
            _quantity = quantity;
            _assetService.LoadEntityIcon(entityTypeId, entityId, Image, GetToken());
            InitShowDoober(showDoober);
        }

        public void InitData(string atlasName, string spriteName, ShowDooberEvent showDoober)
        {
            _assetService.LoadAtlasSpriteInto(atlasName, spriteName, Image, GetToken());
            InitShowDoober(showDoober);
        }

        private void InitShowDoober(ShowDooberEvent showDoober)
        {
            _elapsedTime = 0;
            _offsetAngle = 0;
            _rectTransform = GetComponent<RectTransform>();
            _showDoober = showDoober;
            _startPos = showDoober.StartPosition;
            _endPos = showDoober.EndPosition;
            if (showDoober.SizeScale != 0)
            {
                transform.localScale = Vector3.one * (float)showDoober.SizeScale;
            }
            _rectTransform.position = _startPos;
            PointAtEndPosition();
        }

        public bool FrameUpdateIsComplete(float deltaTime)
        {
            if (_showDoober == null || _showDoober.LerpTime <= 0)
            {
                return true;
            }

            if (_elapsedTime == 0)
            {
                _offsetAngle = MathUtils.FloatRange(0, 360, _rand);
            }
            _elapsedTime += deltaTime;

            float percentDone = MathUtils.Clamp(0, _elapsedTime / _showDoober.LerpTime, 1);

            if (_showDoober.StartOffsetSize > 0)
            {
                float sin = Mathf.Sin(_offsetAngle);
                float cos = Mathf.Cos(_offsetAngle);

                _startPos += new Vector3(cos, sin, 0) * _showDoober.StartOffsetSize * (1 - percentDone) * (1 - percentDone) * 0.25f;
            }


            if (_showDoober.Accelerate)
            {
                percentDone *= percentDone;
            }

            percentDone = MathUtils.Clamp(0, percentDone, 1);

            _rectTransform.position = Vector2.Lerp(_startPos, _endPos, percentDone);


            if (percentDone >= 1)
            {
                _dispatcher.Dispatch(new AddUserCoinVisual() { InstantUpdate = false, QuantityAdded = _quantity, UserCoinTypeId = _entityId });

                return true;
            }

            PointAtEndPosition();

            return false;
        }

        private void PointAtEndPosition()
        {

            if (_showDoober.PointAtEnd)
            {
                Vector2 posDiff = _endPos - _rectTransform.position;

                float angle = Mathf.Atan2(posDiff.y, posDiff.x) * 180 / Mathf.PI;

                _rectTransform.localEulerAngles = new Vector3(0, 0, angle);
            }
        }
    }
}
