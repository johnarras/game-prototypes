using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Quests.Messages;
using Genrpg.Shared.Utils;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Quests.MessageHandlers
{
    public class OnGetQuestsHandler : BaseCharacterServerMapMessageHandler<OnGetQuests>
    {
        protected override async Task InnerProcess(IRandom rand, MapMessagePackage pack, Character ch, OnGetQuests message)
        {
            await Task.CompletedTask;
            ch.AddMessage(message);
        }
    }
}


