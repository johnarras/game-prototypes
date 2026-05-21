using MessagePack;
using OxDb.SharedGame.MapMessages;

namespace OxDb.SharedGame.Inventory.Messages
{
    [MessagePackObject]
    public sealed class OnAddItem : BaseInfrequenMapApiMessage
    {
        [Key(0)] public string UnitId { get; set; }
        [Key(1)] public string ItemId { get; set; }
    }
}


