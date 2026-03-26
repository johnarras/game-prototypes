using Genrpg.Shared.MapMessages;
using Genrpg.Shared.Quests.WorldData;
using MessagePack;
using System.Collections.Generic;

namespace Genrpg.Shared.Quests.Messages
{
    [MessagePackObject]
    public sealed class OnGetQuests : BaseMapApiMessage
    {
        [Key(0)] public string ObjId { get; set; }
        [Key(1)] public List<QuestType> Quests { get; set; }
    }
}


