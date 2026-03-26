using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Movement.Messages;
using Genrpg.Shared.Utils;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Movement.MessageHandlers
{
    public class OnUpdPosHandler : BaseMapObjectServerMapMessageHandler<OnUpdatePos>
    {
        protected override async Task InnerProcess(IRandom rand, MapMessagePackage pack, MapObject obj, OnUpdatePos message)
        {
            if (obj.Id != message.ObjId)
            {
                obj.AddMessage(message);
            }
        }
    }
}


