using Genrpg.Shared.Entities.Entities;
using Genrpg.Shared.Entities.Services;
using Genrpg.Shared.Rewards.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Entities.UI
{
    public class EntityIcon : BaseBehaviour
    {

        public GImage Icon;
        public GText Quantity;

        private long _entityTypeId;
        private long _entityId;
        private long _quantity;
        private long _maxQuantity;
        public void SetData(IReward reward, long maxQuantity = 0)
        {
            SetData(reward.EntityTypeId, reward.EntityId, reward.Quantity, maxQuantity);
        }

        public void SetData(long entityTypeId, long entityId, long quantity, long maxQuantity = 0)
        {
            _assetService.LoadEntityIcon(entityTypeId, entityId, Icon, GetToken());

            if (maxQuantity < 1)
            {
                _uiService.SetText(Quantity, quantity.ToString());
            }
            else
            {
                _uiService.SetText(Quantity, quantity + "/" + maxQuantity);
            }
        }
    }
}
