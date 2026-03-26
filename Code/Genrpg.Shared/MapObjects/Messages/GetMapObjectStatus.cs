using Genrpg.Shared.MapMessages;
using MessagePack;

namespace Genrpg.Shared.MapObjects.Messages
{
    [MessagePackObject]
    public sealed class GetMapObjectStatus : BaseMapApiMessage, IPlayerCommand
    {
        [Key(0)] public string ObjId { get; set; }
    }
}


