using Genrpg.Shared.MapMessages;
using MessagePack;

namespace Genrpg.Shared.Trades.Messages
{
    [MessagePackObject]
    public sealed class OnCancelTrade : BaseInfrequenMapApiMessage
    {
        [Key(0)] public string CharId { get; set; }
        [Key(1)] public string ErrorMessage { get; set; }
    }
}


