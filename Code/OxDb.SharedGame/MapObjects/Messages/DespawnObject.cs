using MessagePack;
using OxDb.SharedGame.MapMessages;

namespace OxDb.SharedGame.MapObjects.Messages
{
    [MessagePackObject]
    public sealed class DespawnObject : BaseMapApiMessage
    {
        [Key(0)] public string ObjId { get; set; }
    }
}


