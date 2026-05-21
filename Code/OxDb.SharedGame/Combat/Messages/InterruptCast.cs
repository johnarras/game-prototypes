using MessagePack;
using OxDb.SharedGame.MapMessages;

namespace OxDb.SharedGame.Combat.Messages
{
    [MessagePackObject]
    public sealed class InterruptCast : BaseMapApiMessage, IPlayerCommand
    {
    }
}


