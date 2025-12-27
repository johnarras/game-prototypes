using Genrpg.MapServer.MapMessaging.Interfaces;
using Genrpg.MapServer.Quests.Services;
using Genrpg.ServerShared.DataStores;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Rewards.Messages;
using Genrpg.Shared.Rewards.Services;
using Genrpg.Shared.Utils;
using System.Collections.Generic;

namespace Genrpg.MapServer.Rewards.Services
{
    public class ServerRewardService : RewardService
    {
        protected IServerQuestService _questService = null;
        protected IMapMessageService _messageService = null;
        protected IFullRepositoryService _fullRepoService = null;
        public override bool GiveRewards<RL>(IRandom rand, MapObject obj, List<RL> resultList, RewardParams rp)
        {
            foreach (RewardList rl in resultList)
            {
                foreach (Reward reward in rl.Rewards)
                {
                    _questService.UpdateQuest(rand, obj, reward);
                }
            }

            return base.GiveRewards(rand, obj, resultList, rp);
        }

        public override void OnAddQuantity<TUpd>(MapObject obj, TUpd upd, long entityTypeId, long entityId, long diff, RewardParams rp)
        {
            if (diff == 0)
            {
                return;
            }
            _fullRepoService.QueueSave(upd);

            if (upd is IOwnerQuantityChild quantityChild)
            {
                OnAddQuantityReward onAdd = new OnAddQuantityReward()
                {
                    CharId = obj.Id,
                    EntityTypeId = entityTypeId,
                    EntityId = entityId,
                    Quantity = diff,
                };

                _messageService.SendMessage(obj, onAdd);
            }
        }
    }
}


