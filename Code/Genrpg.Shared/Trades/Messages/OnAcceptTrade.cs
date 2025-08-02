using MessagePack;
using Genrpg.Shared.MapMessages;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Trades.Messages
{
    [MessagePackObject]
    public sealed class OnAcceptTrade : BaseInfrequenMapApiMessage
    {
        [Key(0)] public string CharId { get; set; }
    }
}
