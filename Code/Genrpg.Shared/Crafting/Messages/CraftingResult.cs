using MessagePack;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Crafting.Entities;

namespace Genrpg.Shared.Crafting.Messages
{
    public class CraftingResult
    {
        public CraftingStats Stats { get; set; }

        public Item CraftedItem { get; set; }

        public bool Succeeded { get; set; }

        public string Message { get; set; }

    }
}


