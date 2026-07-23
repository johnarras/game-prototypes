using OxDb.Client.Entities.UI;
using OxDb.Client.WorldCanvas.Interfaces;
using OxDb.SharedCore.Rewards.Entities;
using OxDb.SharedCore.Utils;
using UnityEngine;

namespace OxDb.Client.Rewards.UI
{
    public class PopupRewardIcon : EntityIcon, IDynamicUIItem
    {
        public float ElapsedSeconds { get; set; }

        private float _displayTime;
        private float _distancePerSecond;
        private float _elapsedTime;

        public void SetData(IReward reward, float displayTime, float distancePerSecond)
        {
            SetEntityData(reward.EntityTypeId, reward.EntityId, reward.Quantity);
            _displayTime = displayTime;
            _distancePerSecond = distancePerSecond;
            _uiService.SetText(QuantityText, reward.Quantity > 1 ? "+" + StrUtils.PrintCommaValue(reward.Quantity) : "");
        }

        public bool FrameUpdateIsComplete(float deltaTime)
        {
            if (GetToken().IsCancellationRequested)
            {
                return true;
            }

            transform.localPosition += new Vector3(0, deltaTime * _distancePerSecond, 0);
            _elapsedTime += deltaTime;


            if (_displayTime > 0 && _elapsedTime >= _displayTime)
            {
                return true;
            }

            return false;
        }
    }
}


