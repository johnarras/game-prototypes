using Genrpg.Shared.MapMessages;
using Genrpg.Shared.Trades.Entities;
using MessagePack;

namespace Genrpg.Shared.Trades.Messages
{
    [MessagePackObject]
    public sealed class OnUpdateTrade : BaseInfrequenMapApiMessage
    {
        [Key(0)] public TradeObject TradeObject { get; set; }
    }
}


