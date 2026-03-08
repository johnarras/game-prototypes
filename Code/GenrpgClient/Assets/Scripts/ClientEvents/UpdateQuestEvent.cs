using Genrpg.Shared.Client.Interfaces;
using Genrpg.Shared.Quests.Entities;
using Genrpg.Shared.Quests.PlayerData;

namespace ClientEvents
{
    public class UpdateQuestEvent : IClientEvent
    {
        public QuestStatus Status;
    }
}


