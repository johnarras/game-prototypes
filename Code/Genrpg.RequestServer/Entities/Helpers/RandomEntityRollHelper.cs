using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.Spawns.Helpers;
using Genrpg.RequestServer.Spawns.Services;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Services;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Spawns.Entities;
using Genrpg.Shared.Utils;

namespace Genrpg.RequestServer.Entities.Helpers
{
    /// <summary>
    /// This lets you pick a random element from a another entity list.
    /// </summary>
    public class RandomEntityRollHelper : BaseWebRollHelper
    {
        private IEntityService _entityService = null;
        private IWebSpawnService _webSpawnService = null;

        public override long HelperKey => EntityTypes.RandomEntity;

        public override async Task<List<Reward>> Roll<SI>(WebContext context, RollLootArgs rollLootArgs, SI si)
        {
            List<Reward> rewards = new List<Reward>();

            List<IIdName> childObjects = _entityService.GetChildList(context.core, si.EntityId);

            if (childObjects.Count < 1)
            {
                return rewards;
            }

            List<IWeightedItem> weightedItems = childObjects.Cast<IWeightedItem>().ToList();

            if (weightedItems.Count > 0)
            {
                double weightSum = weightedItems.Sum(x => x.Weight);
                double weightChosen = context.rand.NextDouble() * weightSum;

                foreach (IWeightedItem weightedItem in weightedItems)
                {
                    weightChosen -= weightedItem.Weight;
                    if (weightChosen <= 0)
                    {
                        IIdName origItem = childObjects.FirstOrDefault(x => x == weightedItem);

                        IWebRollHelper otherRollHelper = _webSpawnService.GetRollHelper(si.EntityId);

                        long quantityMult = 1;

                        if (otherRollHelper != null)
                        {
                            quantityMult = await otherRollHelper.GetQuantityMult(context, rollLootArgs, origItem.IdKey);
                        }

                        rewards.Add(new Reward()
                        {
                            EntityTypeId = si.EntityId,
                            EntityId = origItem.IdKey,
                            QualityTypeId = rollLootArgs.QualityTypeId,
                            Level = rollLootArgs.Level,
                            Quantity = MathUtils.LongRange(si.MinQuantity * quantityMult, si.MaxQuantity * quantityMult, context.rand),
                        });
                        break;
                    }
                }
            }

            await Task.CompletedTask;
            return rewards;
        }
    }
}


