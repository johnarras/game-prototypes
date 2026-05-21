using OxDb.SharedCore.Client.Interfaces;
using OxDb.SharedGame.Quests.PlayerData;

namespace ClientEvents
{
    public class UpdateQuestEvent : IClientEvent
    {
        public QuestStatus Status;
    }
}


