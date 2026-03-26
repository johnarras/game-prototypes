namespace Genrpg.Shared.Quests.Entities
{

    public class AlterQuestType
    {
        public const int Accept = 1;
        public const int Abandon = 2;
        public const int Complete = 3;
    }


    public class AlterQuestStateData
    {
        public long AlterTypeId { get; set; }
        public long QuestTypeId { get; set; }
        public string MapId { get; set; }

        public int MapVersion { get; set; }
        public string QuestGiverId { get; set; }

    }
}


