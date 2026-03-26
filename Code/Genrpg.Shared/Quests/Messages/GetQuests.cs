using Genrpg.Shared.MapMessages;
using MessagePack;

namespace Genrpg.Shared.Quests.Messages
{
    [MessagePackObject]
    public sealed class GetQuests : BaseMapApiMessage, IPlayerCommand
    {
        [Key(0)] public string ObjId { get; set; }
    }
}


