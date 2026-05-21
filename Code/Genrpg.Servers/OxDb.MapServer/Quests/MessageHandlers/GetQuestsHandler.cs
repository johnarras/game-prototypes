using OxDb.MapServer.MapMessaging.MessageHandlers;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Characters.PlayerData;
using OxDb.SharedGame.MapObjects.Entities;
using OxDb.SharedGame.Quests.MapObjectAddons;
using OxDb.SharedGame.Quests.Messages;
using OxDb.SharedGame.Quests.WorldData;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OxDb.MapServer.Quests.MessageHandlers
{
    public class GetQuestsHandler : BaseCharacterServerMapMessageHandler<GetQuests>
    {
        protected override async Task InnerProcess(IRandomContainer rand, Character ch, GetQuests message)
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


