using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Entities.Services;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Rewards.Entities;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Core.PlayerData;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Units;
using OxDb.SharedGame.Spawns.Entities;
using OxDb.SharedGame.Spawns.Services;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace OxDb.SharedGame.Spawns.Helpers
{
    /// <summary>
    /// This lets you pick a random element from a another entity list.
    /// </summary>
    public class RandomEntityRollHelper : BaseRollHelper
    {
        private IEntityService _entityService = null;
        private ISpawnService _spawnService = null;

        public override long HelperKey => EntityTypes.RandomEntity;

        public override async ValueTask<List<RewardList>> Roll<SI>(IUnitDataLookup lookup, SI si, long rewardSourceId, RollLootArgs rollLootArgs)
        {
            List<IIdName> childObjects = _entityService.GetChildList(await lookup.GetAsync<CoreData>(), si.EntityId);

            if (childObjects.Count < 1)
            {
                return _rewardService.CreateListFromList(rewardSourceId, si.EntityId);
            }

            List<Reward> rewards = new List<Reward>();

            List<IWeightedItem> weightedItems = childObjects.Cast<IWeightedItem>().ToList();

            if (weightedItems.Count > 0)
            {
                double weightSum = weightedItems.Sum(x => x.Weight);
                double weightChosen = lookup.Rand.NextDouble() * weightSum;

                foreach (IWeightedItem weightedItem in weightedItems)
                {
                    weightChosen -= weightedItem.Weight;
                    if (weightChosen <= 0)
                    {
                        IIdName origItem = childObjects.FirstOrDefault(x => x == weightedItem);

                        IRollHelper otherRollHelper = _spawnService.GetRollHelper(si.EntityId);

                        long quantityMult = 1;

                        if (otherRollHelper != null)
                        {
                            quantityMult = await otherRollHelper.GetQuantityMult(lookup, rollLootArgs, origItem.IdKey);
                        }

                        rewards.Add(new Reward()
                        {
                            EntityTypeId = si.EntityId,
                            EntityId = origItem.IdKey,
                            Quantity = RandUtils.LongRange(si.MinQuantity * quantityMult, si.MaxQuantity * quantityMult, lookup.Rand),
                        });
                        break;
                    }
                }
            }

            return _rewardService.CreateListFromList(rewardSourceId, si.EntityId, rewards);
        }
    }
}


