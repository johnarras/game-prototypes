using MessagePack;
using OxDb.SharedGame.MapMessages;

namespace OxDb.SharedGame.Spells.Messages
{
    [MessagePackObject]
    public sealed class OnStopCast : BaseMapApiMessage
    {
        [Key(0)] public string CasterId { get; set; }
    }
}


