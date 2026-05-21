using MessagePack;
using OxDb.SharedGame.MapMessages;
using OxDb.SharedGame.Trades.Entities;

namespace OxDb.SharedGame.Trades.Messages
{
    [MessagePackObject]
    public sealed class OnUpdateTrade : BaseInfrequenMapApiMessage
    {
        [Key(0)] public TradeObject TradeObject { get; set; }
    }
}


