using MessagePack;
using OxDb.SharedGame.MapMessages;

namespace OxDb.SharedGame.MapObjects.Messages
{
    [MessagePackObject]
    public sealed class SendSpawn : BaseMapApiMessage
    {
        [Key(0)] public string ToObjId { get; set; }
    }
}


