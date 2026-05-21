using MessagePack;
using OxDb.SharedGame.MapMessages;

namespace OxDb.SharedGame.WhoList.Messages
{
    [MessagePackObject]
    public sealed class GetWhoList : BaseMapApiMessage, IPlayerCommand
    {
        [Key(0)] public string Args { get; set; }
    }
}


