using Genrpg.Shared.MapMessages;
using MessagePack;

namespace Genrpg.Shared.Targets.Messages
{
    [MessagePackObject]
    public sealed class OnTargetIsDead : BaseMapApiMessage
    {
        [Key(0)] public string UnitId { get; set; }
    }
}


