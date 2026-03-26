using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Quests.MapObjectAddons;
using Genrpg.Shared.Quests.Messages;
using Genrpg.Shared.Quests.WorldData;
using Genrpg.Shared.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Quests.MessageHandlers
{
    public class GetQuestsHandler : BaseCharacterServerMapMessageHandler<GetQuests>
    {
        protected override async Task InnerProcess(IRandom rand, MapMessagePackage pack, Character ch, GetQuests message)
        {
            await Task.CompletedTask;
            if (!_objectManager.GetObject(message.ObjId, out MapObject mobject))
            {
                return;
            }

            QuestAddon addon = mobject.GetAddon<QuestAddon>();

            _messageService.SendMessage(ch, new OnGetQuests() { ObjId = message.ObjId, Quests = addon?.Quests ?? new List<QuestType>() }); ;
        }
    }
}


