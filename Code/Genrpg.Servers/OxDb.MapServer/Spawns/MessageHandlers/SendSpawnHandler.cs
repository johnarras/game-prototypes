using OxDb.MapServer.MapMessaging.MessageHandlers;
using OxDb.SharedCore.Serialization.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Characters.PlayerData;
using OxDb.SharedGame.MapObjects.Entities;
using OxDb.SharedGame.MapObjects.Messages;
using System.Threading.Tasks;

namespace OxDb.MapServer.Spawns.MessageHandlers
{
    public class SendSpawnHandler : BaseMapObjectServerMapMessageHandler<SendSpawn>
    {
        private ITextSerializer _serializer = null;
        protected override async Task InnerProcess(IRandomContainer rand, MapObject obj, SendSpawn message)
        {
            await Task.CompletedTask;
            if (!_objectManager.GetChar(message.ToObjId, out Character ch))
            {
                return;
            }

            if (obj.IsDeleted())
            {
                return;
            }

            _messageService.SendMessage(ch, new OnSpawn(obj, _serializer));
        }
    }
}


