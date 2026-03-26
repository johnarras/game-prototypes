using Genrpg.Shared.MapMessages;
using MessagePack;

namespace Genrpg.Shared.Loot.Messages
{
    [MessagePackObject]
    public sealed class ClearLoot : BaseMapApiMessage
    {
        [Key(0)] public string UnitId { get; set; }
    }
}


