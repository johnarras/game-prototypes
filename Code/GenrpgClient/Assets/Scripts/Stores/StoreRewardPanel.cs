using Assets.Scripts.Assets.Sprites.Services;
using Genrpg.Shared.Rewards.Entities;
using System.Threading;

namespace Assets.Scripts.UI.Stores
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

            _spriteService.LoadEntityIcon(reward.EntityTypeId, reward.EntityId, RewardIcon, token);

            _uiService.SetText(RewardQuantity, reward.Quantity.ToString());

        }
    }
}
