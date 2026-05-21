using MessagePack;
using OxDb.SharedGame.MapMessages;

namespace OxDb.SharedGame.Targets.Messages
{
    [MessagePackObject]
    public sealed class OnTargetIsDead : BaseMapApiMessage
    {
        [Key(0)] public string UnitId { get; set; }
    }
}


