using MessagePack;
using OxDb.SharedGame.MapMessages;

namespace OxDb.SharedGame.MapObjects.Messages
{
    [MessagePackObject]
    public sealed class GetSpawnedObject : BaseMapApiMessage, IPlayerCommand
    {
        [Key(0)] public string ObjId { get; set; }
    }
}


