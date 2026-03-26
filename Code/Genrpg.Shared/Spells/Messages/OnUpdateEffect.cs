using Genrpg.Shared.MapMessages;
using MessagePack;

namespace Genrpg.Shared.Spells.Messages
{
    [MessagePackObject]
    public sealed class OnUpdateEffect : BaseMapApiMessage
    {
        [Key(0)] public long Id { get; set; }
        [Key(1)] public float Duration { get; set; }
        [Key(2)] public float DurationLeft { get; set; }
    }
}


