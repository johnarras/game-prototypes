using OxDb.MapServer.MapMessaging.MessageHandlers;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.MapObjects.Entities;
using OxDb.SharedGame.Movement.Messages;
using System.Threading.Tasks;

namespace OxDb.MapServer.Movement.MessageHandlers
{
    public class OnUpdPosHandler : BaseMapObjectServerMapMessageHandler<OnUpdatePos>
    {
        protected override async Task InnerProcess(IRandomContainer rand, MapObject obj, OnUpdatePos message)
        {
            if (obj.Id != message.ObjId)
            {
                obj.AddMessage(message);
            }
        }
    }
}


