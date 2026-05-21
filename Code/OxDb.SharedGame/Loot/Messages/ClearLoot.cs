using MessagePack;
using OxDb.SharedGame.MapMessages;

namespace OxDb.SharedGame.Loot.Messages
{
    [MessagePackObject]
    public sealed class ClearLoot : BaseMapApiMessage
    {
        [Key(0)] public string UnitId { get; set; }
    }
}


