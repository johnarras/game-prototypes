using MessagePack;
namespace Genrpg.Shared.Quests.Entities
{
    public class UpdateQuestResult
    {
        public bool Success { get; set; }

        public string Message { get; set; }

        public UpdateQuestResult()
        {
            Message = "";
        }
    }
}


