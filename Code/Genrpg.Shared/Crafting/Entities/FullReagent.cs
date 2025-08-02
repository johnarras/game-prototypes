using MessagePack;
using Genrpg.Shared.Stats.Settings.Scaling;
using Genrpg.Shared.Crafting.Settings.Recipes;

namespace Genrpg.Shared.Crafting.Entities
{
    [MessagePackObject]
    public class FullReagent
    {
        [Key(0)] public Reagent ReagentMappedTo { get; set; }
        [Key(1)] public ItemPct ItemMappedTo { get; set; }
        [Key(2)] public string ItemId { get; set; }
        [Key(3)] public long ItemTypeId { get; set; }
        [Key(4)] public long QualityTypeId { get; set; }
        [Key(5)] public int Quantity { get; set; }
        [Key(6)] public int Level { get; set; }
    }
}
