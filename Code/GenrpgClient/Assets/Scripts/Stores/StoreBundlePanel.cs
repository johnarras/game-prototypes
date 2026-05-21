
using Assets.Scripts.Stores;
using OxDb.SharedCore.Rewards.Entities;
using OxDb.SharedGame.Purchasing.PlayerData;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.UI.Stores
{
    public class StoreBundlePanel : BaseBehaviour
    {
        public GText Name;
        public GText Description;
        public GameObject RewardAnchor;
        public PurchaseButton PurchaseButton;

        protected List<StoreRewardPanel> _rewards = new List<StoreRewardPanel>();


        protected PlayerBundle _bundle = null;
        protected PlayerStoreOffer _offer = null;
        protected CancellationToken _token;
        protected StoreRewardPanel _rewardPanelPrefab = null;

        public long Index()
        {
            return _bundle.Index;
        }

        public async Task Init(PlayerStoreOffer storeOffer, PlayerBundle bundle, string screenName, StoreRewardPanel rewardPanelPrefab, CancellationToken token)
        {
            _bundle = bundle;
            _offer = storeOffer;
            _token = token;
            _rewardPanelPrefab = rewardPanelPrefab;

            _uiService.SetText(Name, _offer.Name);
            _uiService.SetText(Description, _offer.Desc);

            await PurchaseButton.Init(_offer, bundle);

            _clientEntityService.DestroyAllChildren(RewardAnchor);

            _rewards.Clear();

            if (RewardAnchor != null)
            {

                List<Task> initTasks = new List<Task>();
                foreach (Reward reward in bundle.Rewards)
                {
                    StoreRewardPanel rewardPanel = _clientEntityService.FullInstantiate(_rewardPanelPrefab);

                    _clientEntityService.AddToParent(rewardPanel, RewardAnchor);
                    _rewards.Add(rewardPanel);

                    rewardPanel.Init(reward, token);

                }
            }
        }
    }
}


