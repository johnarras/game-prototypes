using MessagePack;
using OxDb.SharedGame.MapMessages;

namespace OxDb.SharedGame.Spells.Messages
{
    [MessagePackObject]
    public sealed class OnUpdateEffect : BaseMapApiMessage
    {
        [Key(0)] public long Id { get; set; }
        [Key(1)] public float Duration { get; set; }
        [Key(2)] public float DurationLeft { get; set; }
    }
}


