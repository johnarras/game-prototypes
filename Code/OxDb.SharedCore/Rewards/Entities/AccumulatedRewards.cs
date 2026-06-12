using System;
using System.Collections.Generic;
using System.Linq;

namespace OxDb.SharedCore.Rewards.Entities
{



    public class AccumulatedRewards
    {
        public Dictionary<long, List<Reward>> Inflows { get; set; } = new Dictionary<long, List<Reward>>();
        public Dictionary<long, List<Reward>> Outflows { get; set; } = new Dictionary<long, List<Reward>>();


        public void AddLists(List<RewardList> rewardLists)
        {
            foreach (RewardList rlist in rewardLists)
            {
                AddList(rlist);
            }
        }

        public void AddList(RewardList rlist)
        {
            AddRewards(rlist.Rewards, rlist.RewardSourceId);
        }

        public void AddRewards<R>(List<R> rewards, long rewardSourceId) where R : IReward
        {
            foreach (R reward in rewards)
            {
                AddReward(reward, rewardSourceId);
            }
        }
        public void AddReward<R>(R reward, long rewardSourceId) where R : IReward
        {
            AddReward(reward.EntityTypeId, reward.EntityId, reward.Quantity, rewardSourceId);
        }
        public void AddReward(long entityTypeId, long entityId, long quantity, long rewardSourceId)
        {
            if (quantity == 0)
            {
                return;
            }

            Dictionary<long, List<Reward>> dict = (quantity > 0 ? Inflows : Outflows);

            if (!dict.TryGetValue(rewardSourceId, out List<Reward> rewards))
            {
                rewards = new List<Reward>();
                dict[rewardSourceId] = rewards;
            }

            Reward rew = rewards.FirstOrDefault(x => x.EntityTypeId == entityTypeId && x.EntityId == entityId);

            if (rew == null)
            {
                rew = new Reward()
                {
                    EntityTypeId = entityTypeId,
                    EntityId = entityId,
                };
                rewards.Add(rew);
            }

            rew.Quantity += Math.Abs(quantity);
        }
    }
}
