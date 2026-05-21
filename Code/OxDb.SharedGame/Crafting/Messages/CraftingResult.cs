using OxDb.SharedGame.Crafting.Entities;
using OxDb.SharedGame.Inventory.PlayerData;

namespace OxDb.SharedGame.Crafting.Messages
{
    public class CraftingResult
    {
        public CraftingStats Stats { get; set; }

        public Item CraftedItem { get; set; }

        public bool Succeeded { get; set; }

        public string Message { get; set; }

    }
}


