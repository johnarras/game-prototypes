using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Spells.Messages;
using Genrpg.Shared.Utils;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Spells.MessageHandlers
{
    public class OnRemoveEffectHandler : BaseMapObjectServerMapMessageHandler<OnRemoveEffect>
    {
        protected override async Task InnerProcess(IRandom rand, MapMessagePackage pack, MapObject obj, OnRemoveEffect message)
        {
            obj.AddMessage(message);
            await Task.CompletedTask;
        }
    }
}


