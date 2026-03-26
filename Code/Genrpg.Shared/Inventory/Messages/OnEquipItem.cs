using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.MapMessages;
using MessagePack;

namespace Genrpg.Shared.Inventory.Messages
{
    [MessagePackObject]
    public sealed class OnEquipItem : BaseInfrequenMapApiMessage
    {
        [Key(0)] public string UnitId { get; set; }
        [Key(1)] public Item Item { get; set; }
    }
}


