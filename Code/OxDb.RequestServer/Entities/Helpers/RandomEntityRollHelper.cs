using OxDb.RequestServer.Core;
using OxDb.RequestServer.Spawns.Helpers;
using OxDb.RequestServer.Spawns.Services;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Entities.Services;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Rewards.Entities;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Core.PlayerData;
using OxDb.SharedGame.Spawns.Entities;

namespace OxDb.RequestServer.Entities.Helpers
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

            List<IIdName> childObjects = _entityService.GetChildList(await context.GetAsync<CoreData>(), si.EntityId);

            if (childObjects.Count < 1)
            {
                return rewards;
            }

            List<IWeightedItem> weightedItems = childObjects.Cast<IWeightedItem>().ToList();

            if (weightedItems.Count > 0)
            {
                double weightSum = weightedItems.Sum(x => x.Weight);
                double weightChosen = context.Rand.NextDouble() * weightSum;

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
                            Quantity = RandUtils.LongRange(si.MinQuantity * quantityMult, si.MaxQuantity * quantityMult, context.Rand),
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


