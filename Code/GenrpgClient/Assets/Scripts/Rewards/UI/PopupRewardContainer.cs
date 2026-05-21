using Assets.Scripts.Assets.Constants;
using Assets.Scripts.Doobers.Events;
using Assets.Scripts.DynamicUI.Services;
using Assets.Scripts.WorldCanvas.GameEvents;
using OxDb.SharedCore.Rewards.Entities;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts.Rewards.UI
{


    public class RewardPosition
    {
        public IReward Reward;
        public Vector3 Position;
    }
    public class PopupRewardContainer : BaseBehaviour
    {

        protected IDynamicUIService _dynamicUIService = null;

        public float DistancePerSecond;
        public float DisplayTime;

        public void ShowReward(long entityTypeId, long entityId, long quantity)
        {
            ShowReward(new Reward() { EntityTypeId = entityTypeId, EntityId = entityId, Quantity = quantity });
        }

        public void ShowReward(IReward reward)
        {
            _assetService.LoadAssetInto(gameObject, AssetCategoryNames.UI, "PopupRewardIcon", OnLoadIcon, GetToken(), reward,
                "Rewards");


            DooberArgs dooberArgs = _dynamicUIService.CheckoutDooberArgs();

            dooberArgs.EntityTypeId = reward.EntityTypeId;
            dooberArgs.EntityId = reward.EntityId;
            dooberArgs.Quantity = reward.Quantity;
            dooberArgs.StartPosition = gameObject.transform.position;
            dooberArgs.LerpTime = 1.5f;

            _dynamicUIService.ShowDoober(dooberArgs);
        }

        private void OnLoadIcon(GameObject go, IReward rew, CancellationToken token)
        {
            PopupRewardIcon icon = go.GetComponent<PopupRewardIcon>();

            if (icon == null)
            {
                _clientEntityService.Destroy(go);
                return;
            }

            icon.SetData(rew, DisplayTime, DistancePerSecond);

            _dispatcher.Dispatch(new DynamicUIItem(icon.gameObject, icon, transform.position, DynamicUILocation.WorldSpace));
        }
    }
}


