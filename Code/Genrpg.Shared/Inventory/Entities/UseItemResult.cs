using Genrpg.Shared.Inventory.PlayerData;
using MessagePack;
namespace Genrpg.Shared.Inventory.Entities
{
    public class UseItemResult
    {
        public bool Success { get; set; }
        public Item ItemUsed { get; set; }
        public object ResultObject { get; set; }
        public string Message { get; set; }
    }
}


