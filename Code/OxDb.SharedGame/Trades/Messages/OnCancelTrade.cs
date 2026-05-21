using MessagePack;
using OxDb.SharedGame.MapMessages;

namespace OxDb.SharedGame.Trades.Messages
{
    [MessagePackObject]
    public sealed class OnCancelTrade : BaseInfrequenMapApiMessage
    {
        [Key(0)] public string CharId { get; set; }
        [Key(1)] public string ErrorMessage { get; set; }
    }
}


