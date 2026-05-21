using MessagePack;
using OxDb.SharedGame.MapMessages;

namespace OxDb.SharedGame.Movement.Messages
{
    [MessagePackObject]
    public sealed class OnAddToGrid : BaseMapApiMessage
    {
        [Key(0)] public string UserId { get; set; }
        [Key(1)] public int GridX { get; set; }
        [Key(2)] public int GridZ { get; set; }
    }
}


