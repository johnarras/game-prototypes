using OxDb.MapServer.MapMessaging.MessageHandlers;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Characters.PlayerData;
using OxDb.SharedGame.Quests.Messages;
using System.Threading.Tasks;

namespace OxDb.MapServer.Quests.MessageHandlers
{
    public class OnGetQuestsHandler : BaseCharacterServerMapMessageHandler<OnGetQuests>
    {
        protected override async Task InnerProcess(IRandomContainer rand, Character ch, OnGetQuests message)
        {
            await Task.CompletedTask;
            ch.AddMessage(message);
        }
    }
}


