using MessagePack;
namespace OxDb.SharedGame.Quests.WorldData
{
    [MessagePackObject]
    public class QuestTaskStatus
    {
        [Key(0)] public int Index { get; set; }
        [Key(1)] public long CurrQuantity { get; set; }
    }
}


