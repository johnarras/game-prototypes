using MessagePack;
using OxDb.SharedGame.MapMessages;

namespace OxDb.SharedGame.Ftue.Messages
{
    [MessagePackObject]
    public sealed class CompleteFtueStepMessage : BaseInfrequenMapApiMessage, IPlayerCommand
    {
        [Key(0)] public long FtueStepId { get; set; }
    }
}


