using Genrpg.Shared.MapMessages;
using MessagePack;

namespace Genrpg.Shared.RpgLevels.Messages
{
    [MessagePackObject]
    public sealed class NewRpgLevel : BaseMapApiMessage
    {
        [Key(0)] public string UnitId { get; set; }
        [Key(1)] public long Level { get; set; }
    }
}


