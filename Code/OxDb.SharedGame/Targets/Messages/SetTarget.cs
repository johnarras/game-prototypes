using MessagePack;
using OxDb.SharedGame.MapMessages;

namespace OxDb.SharedGame.Targets.Messages
{
    [MessagePackObject]
    public sealed class SetTarget : BaseMapApiMessage, IPlayerCommand
    {
        [Key(0)] public string TargetId { get; set; }
    }
}


