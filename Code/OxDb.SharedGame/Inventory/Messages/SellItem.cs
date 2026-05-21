using MessagePack;
using OxDb.SharedGame.MapMessages;

namespace OxDb.SharedGame.Inventory.Messages
{
    [MessagePackObject]
    public sealed class SellItem : BaseInfrequenMapApiMessage, IPlayerCommand
    {
        [Key(0)] public string ItemId { get; set; }
        [Key(1)] public string UnitId { get; set; }
    }
}


