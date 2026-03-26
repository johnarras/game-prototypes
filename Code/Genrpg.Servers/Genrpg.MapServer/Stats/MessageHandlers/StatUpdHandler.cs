using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Stats.Messages;
using Genrpg.Shared.Utils;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Stats.MessageHandlers
{
    public class StatUpdHandler : BaseMapObjectServerMapMessageHandler<StatUpd>
    {
        protected override async Task InnerProcess(IRandom rand, MapMessagePackage pack, MapObject obj, StatUpd message)
        {
            obj.AddMessage(message);
        }
    }
}


