using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Chat.Messages;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Utils;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Chat.MessageHandlers
{
    public class OnChatMessageHandler : BaseCharacterServerMapMessageHandler<OnChatMessage>
    {
        protected override async Task InnerProcess(IRandom rand, MapMessagePackage pack, Character ch, OnChatMessage message)
        {
            ch.AddMessage(message);
            await Task.CompletedTask;
        }
    }
}


