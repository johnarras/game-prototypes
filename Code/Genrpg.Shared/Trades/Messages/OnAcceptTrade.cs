using Genrpg.Shared.MapMessages;
using MessagePack;

namespace Genrpg.Shared.Trades.Messages
{
    [MessagePackObject]
    public sealed class OnAcceptTrade : BaseInfrequenMapApiMessage
    {
        [Key(0)] public string CharId { get; set; }
    }
}


