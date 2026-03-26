using Genrpg.Shared.Crafting.Settings.Recipes;
using Genrpg.Shared.Stats.Settings.Scaling;

namespace Genrpg.Shared.Crafting.Entities
{
    public class FullReagent
    {
        public Reagent ReagentMappedTo { get; set; }
        public ItemPct ItemMappedTo { get; set; }
        public string ItemId { get; set; }
        public long ItemTypeId { get; set; }
        public long QualityTypeId { get; set; }
        public int Quantity { get; set; }
        public long Level { get; set; }
    }
}


