using Genrpg.Shared.MapMessages;
using MessagePack;

namespace Genrpg.Shared.Targets.Messages
{
    [MessagePackObject]
    public sealed class SetTarget : BaseMapApiMessage, IPlayerCommand
    {
        [Key(0)] public string TargetId { get; set; }
    }
}


