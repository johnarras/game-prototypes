using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Targets.Messages;
using Genrpg.Shared.Utils;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Units.MessageHandlers
{
    public class OnSetTargetHandler : BaseMapObjectServerMapMessageHandler<OnSetTarget>
    {
        protected override async Task InnerProcess(IRandom rand, MapMessagePackage pack, MapObject obj, OnSetTarget message)
        {
            obj.AddMessage(message);
        }
    }
}


