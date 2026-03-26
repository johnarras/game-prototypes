using Genrpg.Shared.MapMessages;
using MessagePack;

namespace Genrpg.Shared.Ftue.Messages
{
    [MessagePackObject]
    public sealed class CompleteFtueStepMessage : BaseInfrequenMapApiMessage, IPlayerCommand
    {
        [Key(0)] public long FtueStepId { get; set; }
    }
}


