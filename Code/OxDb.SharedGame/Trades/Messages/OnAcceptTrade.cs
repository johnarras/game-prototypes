using MessagePack;
using OxDb.SharedGame.MapMessages;

namespace OxDb.SharedGame.Trades.Messages
{
    [MessagePackObject]
    public sealed class OnAcceptTrade : BaseInfrequenMapApiMessage
    {
        [Key(0)] public string CharId { get; set; }
    }
}


