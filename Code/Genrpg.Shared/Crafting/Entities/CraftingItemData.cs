using MessagePack;
using System.Collections.Generic;

namespace Genrpg.Shared.Crafting.Entities
{
    public class CraftingItemData
    {
        public long RecipeTypeId { get; set; }
        public long ScalingTypeId { get; set; }
        public FullReagent BaseScalingReagent { get; set; }
        public List<FullReagent> StatReagents { get; set; } = new List<FullReagent>();
        public List<FullReagent> LevelQualityReagents { get; set; } = new List<FullReagent>();


        public List<FullReagent> GetAllReagents()
        {
            List<FullReagent> retval = new List<FullReagent>();

            retval.Add(BaseScalingReagent);
            retval.AddRange(StatReagents);
            retval.AddRange(LevelQualityReagents);

            return retval;
        }
    }
}


