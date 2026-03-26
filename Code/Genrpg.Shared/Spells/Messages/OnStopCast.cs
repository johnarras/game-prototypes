using Genrpg.Shared.MapMessages;
using MessagePack;

namespace Genrpg.Shared.Spells.Messages
{
    [MessagePackObject]
    public sealed class OnStopCast : BaseMapApiMessage
    {
        [Key(0)] public string CasterId { get; set; }
    }
}


