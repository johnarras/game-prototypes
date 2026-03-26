
using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.MapServer.MapMessaging.Services;
using Genrpg.ServerShared.CloudComms.Constants;
using Genrpg.ServerShared.CloudComms.Servers.PlayerServer.Queues;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.MapServer.Messages;
using Genrpg.Shared.Movement.Messages;
using Genrpg.Shared.Utils;
using System;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Movement.MessageHandlers
{
    public class UpdPosHandler : BaseMapObjectServerMapMessageHandler<UpdatePos>
    {
        protected override async Task InnerProcess(IRandom rand, MapMessagePackage pack, MapObject obj, UpdatePos message)
        {
            obj.X = message.GetX();
            obj.Y = message.GetY();
            obj.Z = message.GetZ();
            obj.Rot = message.GetRot();
            obj.Speed = message.GetSpeed();
            obj.PrevZoneId = obj.ZoneId;
            obj.ZoneId = message.ZoneId;

            if (obj is Character ch)
            {
                if (obj.PrevZoneId != obj.ZoneId)
                {
                    _cloudCommsService.SendQueueMessage(CloudServerNames.Player, new PlayerEnterZone() { Id = ch.Id, ZoneId = ch.ZoneId });
                }


                if ((DateTime.UtcNow - ch.LastServerStatTime).TotalSeconds > 5)
                {
                    MapMessageService serverMessageService = _messageService as MapMessageService;

                    ServerMessageCounts counts = serverMessageService.GetCounts();

                    counts.MapCounts = _objectManager.GetCounts();

                    obj.AddMessage(counts);
                    ch.LastServerStatTime = DateTime.UtcNow;
                }
            }
            _objectManager.UpdatePosition(rand, obj, message.GetKeysDown());

        }
    }
}


