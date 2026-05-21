using MessagePack;
using OxDb.SharedGame.MapMessages;
namespace OxDb.SharedGame.Trades.Messages
{
    [MessagePackObject]
    public sealed class CancelTrade : BaseInfrequenMapApiMessage, IPlayerCommand
    {
        [Key(0)] public string CharId { get; set; }
    }
}


