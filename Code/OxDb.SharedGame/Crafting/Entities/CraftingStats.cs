using System.Collections.Generic;

namespace OxDb.SharedGame.Crafting.Entities
{
    public class CraftingStats
    {
        public long EntityTypeId { get; set; }
        public long EntityId { get; set; }
        public List<CraftingStat> Stats { get; set; } = new List<CraftingStat>();
        public long Level { get; set; }
        public long QualityTypeId { get; set; }
        public long RecipeTypeId { get; set; }
        public long ScalingTypeId { get; set; }
        public int ReagentQuantity { get; set; }

        public CraftingItemData Data { get; set; }

        public bool IsValid { get; set; }

        public string Message { get; set; }

    }

    public class CraftingStat
    {
        public short Id { get; set; }
        public int Val { get; set; }
    }
}


