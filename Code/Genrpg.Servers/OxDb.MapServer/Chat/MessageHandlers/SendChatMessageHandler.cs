using OxDb.MapServer.MapMessaging.MessageHandlers;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Characters.PlayerData;
using OxDb.SharedGame.Chat.Constants;
using OxDb.SharedGame.Chat.Messages;
using OxDb.SharedGame.MapObjects.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OxDb.MapServer.Chat.MessageHandlers
{
    public class SendChatMessageHandler : BaseMapObjectServerMapMessageHandler<SendChatMessage>
    {
        protected override async ValueTask InnerProcess(MapObject obj, SendChatMessage message)
        {
            float radius = 0;
            if (message.ChatTypeId == ChatTypes.Say)
            {
                radius = 20;
            }
            else if (message.ChatTypeId == ChatTypes.Yell)
            {
                radius = 50;
            }

            if (radius > 0)
            {
                List<Character> nearbyChars = _objectManager.GetTypedObjectsNear<Character>(obj.X, obj.Z, null, radius, true);

                OnChatMessage onChatMessage = new OnChatMessage()
                {
                    SenderId = obj.Id,
                    SenderName = obj.Name,
                    ChatTypeId = message.ChatTypeId,
                    Message = message.Text,
                };
                foreach (Character ch in nearbyChars)
                {
                    _messageService.SendMessage(ch, onChatMessage);
                }
            }
            await Task.CompletedTask;
        }
    }
}


