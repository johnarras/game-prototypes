using Genrpg.Shared.MapMessages;
using Genrpg.Shared.Trades.Constants;
using MessagePack;

namespace Genrpg.Shared.Trades.Messages
{
    [MessagePackObject]
    public sealed class UpdateTrade : BaseInfrequenMapApiMessage, IPlayerCommand
    {
        [Key(0)] public string[] ItemIds { get; set; } = new string[TradeConstants.MaxItems];
        [Key(1)] public long Money { get; set; }
    }
}


