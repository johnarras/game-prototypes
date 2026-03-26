using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapObjects.Messages;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Serialization.Interfaces;
using Genrpg.Shared.Utils;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Spawns.MessageHandlers
{
    public class SendSpawnHandler : BaseMapObjectServerMapMessageHandler<SendSpawn>
    {
        private ITextSerializer _serializer = null;
        protected override async Task InnerProcess(IRandom rand, MapMessagePackage pack, MapObject obj, SendSpawn message)
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


