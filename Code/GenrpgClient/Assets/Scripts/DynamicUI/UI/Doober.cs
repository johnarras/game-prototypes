
using Assets.Scripts.ClientEvents.UserCoins;
using Assets.Scripts.WorldCanvas.Interfaces;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Utils;
using UnityEngine;

namespace Assets.Scripts.Doobers.UI
{
    public class Doober : BaseBehaviour, IDynamicUIItem
    {
        public GImage Image;

        private long _entityTypId = 0;
        private long _entityId = 0;
        private long _quantity = 0;

        private Vector2 _startPos;
        private Vector2 _endPos;

        RectTransform _rectTransform;

        public void InitData(long entityTypeId, long entityId, long quantity, Vector2 startPos, Vector2 endPos)
        {
            _entityTypId = entityTypeId;
            _entityId = entityId;
            _quantity = quantity;
            _startPos = startPos;
            _endPos = endPos;
            _assetService.LoadEntityIcon(entityTypeId, entityId, Image, GetToken());
            _rectTransform = GetComponent<RectTransform>();
            offAngle = MathUtils.FloatRange(0, 360, _rand);
            offSpeed = MathUtils.FloatRange(30, 50, _rand);
        }

        public void InitData(string atlasName, string spriteName, Vector2 startPos, Vector2 endPos)
        {
            _startPos = startPos;
            _endPos = endPos;
            _assetService.LoadAtlasSpriteInto(atlasName, spriteName, Image, GetToken());
            _rectTransform = GetComponent<RectTransform>();
            lerpAccel = 0.0f;
            lerpSpeed = 0.2f;
        }

        float lerpAccel = 0.002f;
        float lerpPct = 0.0f;
        float lerpSpeed = 0;

        float offAngle = 0;
        float offSpeed = 0;
        public bool FrameUpdateIsComplete(float deltaTime)
        {

            offSpeed /= 2;

            if (offSpeed > 0.001f)
            {
                float sin = Mathf.Sin(offAngle);
                float cos = Mathf.Cos(offAngle);

                _startPos += new Vector2(cos, sin) * offSpeed;
            }



            lerpSpeed += lerpAccel;
            lerpPct += lerpSpeed;
            if (lerpPct > 1)
            {
                lerpPct = 1;
            }

            _rectTransform.position = Vector3.Lerp(_startPos, _endPos, lerpPct);

            if (lerpPct == 1)
            {
                _dispatcher.Dispatch(new AddUserCoinVisual() { InstantUpdate = false, QuantityAdded = _quantity, UserCoinTypeId = _entityId });
              
            }


            return lerpPct >= 1;
        }

    }
}
