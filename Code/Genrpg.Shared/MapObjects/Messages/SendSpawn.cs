using Genrpg.Shared.MapMessages;
using MessagePack;

namespace Genrpg.Shared.MapObjects.Messages
{
    [MessagePackObject]
    public sealed class SendSpawn : BaseMapApiMessage
    {
        [Key(0)] public string ToObjId { get; set; }
    }
}


