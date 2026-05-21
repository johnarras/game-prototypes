using MessagePack;
using OxDb.SharedGame.Inventory.PlayerData;
using OxDb.SharedGame.MapMessages;

namespace OxDb.SharedGame.Inventory.Messages
{
    [MessagePackObject]
    public sealed class OnEquipItem : BaseInfrequenMapApiMessage
    {
        [Key(0)] public string UnitId { get; set; }
        [Key(1)] public Item Item { get; set; }
    }
}


