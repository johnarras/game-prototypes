using MessagePack;
using OxDb.SharedGame.MapMessages;

namespace OxDb.SharedGame.Inventory.Messages
{
    [MessagePackObject]
    public sealed class UnequipItem : BaseInfrequenMapApiMessage, IPlayerCommand
    {
        [Key(0)] public string ItemId { get; set; }
    }
}


