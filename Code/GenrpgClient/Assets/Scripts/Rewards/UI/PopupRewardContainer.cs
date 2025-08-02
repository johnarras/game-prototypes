using Assets.Scripts.Doobers.Events;
using Assets.Scripts.WorldCanvas.GameEvents;
using Genrpg.Shared.Client.Assets.Constants;
using Genrpg.Shared.Rewards.Entities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        private IInputService _inputService;

        public float DistancePerSecond;
        public float DisplayTime;

        public void ShowReward (long entityTypeId, long entityId, long quantity)
        {
            ShowReward(new Reward() { EntityTypeId = entityTypeId, EntityId = entityId, Quantity = quantity });
        }

        public void ShowReward(IReward reward)
        {
            _assetService.LoadAssetInto(gameObject, AssetCategoryNames.UI, "PopupRewardIcon", OnLoadIcon, reward, GetToken(),
                "Rewards");

            _dispatcher.Dispatch(new ShowDoober()
            {
                EntityTypeId = reward.EntityTypeId,
                EntityId = reward.EntityId,
                Quantity = reward.Quantity,
                StartPosition = gameObject.transform.position,

            });
        }

        private void OnLoadIcon(object obj, object data, CancellationToken token)
        {
            GameObject go = obj as GameObject;

            if (go == null)
            {
                return;
            }

            IReward rew = data as IReward;

            if (rew == null)
            {
                return;
            }

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
