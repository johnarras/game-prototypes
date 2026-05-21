using MessagePack;
using OxDb.SharedGame.MapMessages;
using OxDb.SharedGame.Quests.WorldData;
using System.Collections.Generic;

namespace OxDb.SharedGame.Quests.Messages
{
    [MessagePackObject]
    public sealed class OnGetQuests : BaseMapApiMessage
    {
        [Key(0)] public string ObjId { get; set; }
        [Key(1)] public List<QuestType> Quests { get; set; }
    }
}


