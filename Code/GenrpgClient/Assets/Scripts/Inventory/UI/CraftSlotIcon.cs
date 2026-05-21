using OxDb.SharedGame.Inventory.PlayerData;

/// <summary>
/// Use this to track items that are currently being added to a recipe.
/// </summary>
public class CraftSlotIcon : BaseBehaviour
{
    public Item item { get; set; }
    public long quantity;
    public string description;
}



