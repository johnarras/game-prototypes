using Genrpg.Shared.MapMessages;
using MessagePack;

namespace Genrpg.Shared.Inventory.Messages
{
    [MessagePackObject]
    public sealed class UnequipItem : BaseInfrequenMapApiMessage, IPlayerCommand
    {
        [Key(0)] public string ItemId { get; set; }
    }
}


