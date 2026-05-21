using OxDb.SharedCore.Rewards.Entities;
using OxDb.SharedGame.Quests.PlayerData;
using System.Collections.Generic;

namespace OxDb.SharedGame.Quests.Entities
{
    public class AlterQuestStateResult
    {
        public long AlterTypeId { get; set; }
        public QuestStatus Status { get; set; }

        public List<Reward> Rewards { get; set; }

        public string Message { get; set; }

        public bool Success { get; set; }
    }
}


