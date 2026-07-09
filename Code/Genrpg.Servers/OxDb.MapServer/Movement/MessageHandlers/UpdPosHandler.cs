using OxDb.MapServer.MapMessaging.MessageHandlers;
using OxDb.MapServer.MapMessaging.Services;
using OxDb.ServerCore.CloudComms.Servers.PlayerServer.Queues;
using OxDb.ServerCore.Constants;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Characters.PlayerData;
using OxDb.SharedGame.MapObjects.Entities;
using OxDb.SharedGame.MapServer.Messages;
using OxDb.SharedGame.Movement.Messages;
using System;
using System.Threading.Tasks;

namespace OxDb.MapServer.Movement.MessageHandlers
{
    public class UpdPosHandler : BaseMapObjectServerMapMessageHandler<UpdatePos>
    {
        protected override async ValueTask InnerProcess(MapObject obj, UpdatePos message)
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
                    _cloudCommsService.SendQueueMessage(ServerNames.Player, new PlayerEnterZone() { Id = ch.Id, ZoneId = ch.ZoneId });
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
            _objectManager.UpdatePosition(obj, message.GetKeysDown());

        }
    }
}


