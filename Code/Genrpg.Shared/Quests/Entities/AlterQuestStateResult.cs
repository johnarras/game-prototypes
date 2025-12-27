using MessagePack;
using System.Collections.Generic;
using Genrpg.Shared.Quests.PlayerData;
using Genrpg.Shared.Rewards.Entities;

namespace Genrpg.Shared.Quests.Entities
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


