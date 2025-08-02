using Assets.Scripts.WorldCanvas.Interfaces;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Rewards.UI
{
    public class PopupRewardIcon : BaseBehaviour, IDynamicUIItem
    {
        public float ElapsedSeconds { get; set; }
        public GImage Icon;
        public GText Quantity;

        private float _displayTime;
        private float _distancePerSecond;
        private float _elapsedTime;

        public void SetData(IReward reward, float displayTime, float distancePerSecond)
        {
            _displayTime = displayTime;
            _distancePerSecond = distancePerSecond;
            _uiService.SetText(Quantity, reward.Quantity > 1 ? "+" + StrUtils.PrintCommaValue(reward.Quantity) : "");
            _assetService.LoadEntityIcon(reward.EntityTypeId, reward.EntityId, Icon, GetToken());
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
