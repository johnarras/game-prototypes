using Genrpg.Shared.MapMessages;
using MessagePack;

namespace Genrpg.Shared.Inventory.Messages
{
    [MessagePackObject]
    public sealed class EquipItem : BaseInfrequenMapApiMessage, IPlayerCommand
    {
        [Key(0)] public string ItemId { get; set; }
        [Key(1)] public long EquipSlot { get; set; }
    }
}


