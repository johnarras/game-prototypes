using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.Spawns.Helpers;
using Genrpg.RequestServer.Spawns.Services;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Services;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Inventory.Constants;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Spawns.Entities;
using Genrpg.Shared.Spawns.Settings;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.RequestServer.Entities.Helpers
{
    /// <summary>
    /// This lets you pick a random element from a another entity list.
    /// </summary>
    public class RandomEntityRollHelper : BaseWebRollHelper
    {
        private IEntityService _entityService = null;
        private IWebSpawnService _webSpawnService = null;

        public override long Key => EntityTypes.RandomEntity;

        public override async Task<List<Reward>> Roll<SI>(WebContext context, RollData rollData, SI si)
        {
            List<Reward> rewards = new List<Reward>();

            List<IIdName> childObjects = _entityService.GetChildList(context.user, si.EntityId);

            if (childObjects.Count < 1)
            {
                return rewards;
            }



            List<IWeightedItem> weightedItems = childObjects.Cast<IWeightedItem>().ToList();

            if (weightedItems.Count > 0)
            {
                double weightSum = weightedItems.Sum(x=>x.Weight);
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
                            quantityMult = await otherRollHelper.GetQuantityMult(context, rollData, origItem.IdKey);
                        }

                        rewards.Add(new Reward()
                        {
                            EntityTypeId = si.EntityId,
                            EntityId = origItem.IdKey,
                            QualityTypeId = rollData.QualityTypeId,
                            Level = rollData.Level,
                            Quantity = MathUtils.LongRange(si.MinQuantity*quantityMult, si.MaxQuantity*quantityMult, context.rand),
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
