using MessagePack;
using OxDb.SharedGame.MapMessages;

namespace OxDb.SharedGame.Trades.Messages
{
    [MessagePackObject]
    public sealed class StartTrade : BaseInfrequenMapApiMessage, IPlayerCommand
    {
        [Key(0)] public string CharId { get; set; }
    }
}


