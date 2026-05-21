using MessagePack;
using OxDb.SharedGame.MapMessages;

namespace OxDb.SharedGame.Trades.Messages
{
    [MessagePackObject]
    public sealed class OnStartTrade : BaseInfrequenMapApiMessage
    {
        [Key(0)] public string CharId { get; set; }
        [Key(1)] public string Name { get; set; }
    }
}


