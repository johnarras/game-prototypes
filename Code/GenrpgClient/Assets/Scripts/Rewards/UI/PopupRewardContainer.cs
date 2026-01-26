using Assets.Scripts.Doobers.Events;
using Assets.Scripts.WorldCanvas.GameEvents;
using Genrpg.Shared.Client.Assets.Constants;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Utils;
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

            _dispatcher.Dispatch(new ShowDooberEvent()
            {
                EntityTypeId = reward.EntityTypeId,
                EntityId = reward.EntityId,
                Quantity = reward.Quantity,
                StartPosition = gameObject.transform.position,
                LerpTime = 1.5f,
                Accelerate = true,
                StartOffsetSize = MathUtil.FloatRange(0, 100, _rand)

            });
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


