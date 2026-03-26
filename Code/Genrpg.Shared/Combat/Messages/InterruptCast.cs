using Genrpg.Shared.MapMessages;
using MessagePack;

namespace Genrpg.Shared.Combat.Messages
{
    [MessagePackObject]
    public sealed class InterruptCast : BaseMapApiMessage, IPlayerCommand
    {
    }
}


