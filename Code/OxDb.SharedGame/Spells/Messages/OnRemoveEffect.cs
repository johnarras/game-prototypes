using MessagePack;
using OxDb.SharedGame.MapMessages;

namespace OxDb.SharedGame.Spells.Messages
{
    [MessagePackObject]
    public sealed class OnRemoveEffect : BaseMapApiMessage
    {
        [Key(0)] public string TargetId { get; set; }
        [Key(1)] public long Id { get; set; }
    }
}


