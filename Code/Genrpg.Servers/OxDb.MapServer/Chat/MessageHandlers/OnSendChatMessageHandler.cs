using OxDb.MapServer.MapMessaging.MessageHandlers;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Characters.PlayerData;
using OxDb.SharedGame.Chat.Messages;
using System.Threading.Tasks;

namespace OxDb.MapServer.Chat.MessageHandlers
{
    public class OnChatMessageHandler : BaseCharacterServerMapMessageHandler<OnChatMessage>
    {
        protected override async Task InnerProcess(IRandomContainer rand, Character ch, OnChatMessage message)
        {
            ch.AddMessage(message);
            await Task.CompletedTask;
        }
    }
}


