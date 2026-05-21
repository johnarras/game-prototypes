using MessagePack;
using OxDb.SharedGame.MapMessages;
using OxDb.SharedGame.Trades.Constants;

namespace OxDb.SharedGame.Trades.Messages
{
    [MessagePackObject]
    public sealed class UpdateTrade : BaseInfrequenMapApiMessage, IPlayerCommand
    {
        [Key(0)] public string[] ItemIds { get; set; } = new string[TradeConstants.MaxItems];
        [Key(1)] public long Money { get; set; }
    }
}


