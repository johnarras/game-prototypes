using OxDb.Client.Assets.Sprites.Services;
using OxDb.SharedCore.Rewards.Entities;
using System.Threading;

namespace OxDb.Client.UI.Stores
{
    public class StoreRewardPanel : BaseBehaviour
    {
        protected ISpriteService _spriteService = null;

        public GImage RewardIcon;
        public GText RewardQuantity;

        private Reward _spawnItem;
        public void Init(Reward reward, CancellationToken token)
        {
            _spawnItem = reward;

            _spriteService.SetEntityIcon(reward.EntityTypeId, reward.EntityId, RewardIcon, token);

            _uiService.SetText(RewardQuantity, reward.Quantity.ToString());

        }
    }
}


