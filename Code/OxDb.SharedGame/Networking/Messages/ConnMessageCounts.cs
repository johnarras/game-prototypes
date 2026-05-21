using MessagePack;
using OxDb.SharedGame.MapMessages;

namespace OxDb.SharedGame.Networking.Messages
{
    [MessagePackObject]
    public sealed class ConnMessageCounts : BaseMapApiMessage
    {
        [Key(0)] public long MessagesSent { get; set; }
        [Key(1)] public long MessagesReceived { get; set; }
        [Key(2)] public long BytesSent { get; set; }
        [Key(3)] public long BytesReceived { get; set; }
        [Key(4)] public long Seconds { get; set; }
    }
}


