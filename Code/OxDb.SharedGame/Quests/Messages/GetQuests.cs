using MessagePack;
using OxDb.SharedGame.MapMessages;

namespace OxDb.SharedGame.Quests.Messages
{
    [MessagePackObject]
    public sealed class GetQuests : BaseMapApiMessage, IPlayerCommand
    {
        [Key(0)] public string ObjId { get; set; }
    }
}


