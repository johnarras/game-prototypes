using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.RpgLevels.Messages;
using Genrpg.Shared.Utils;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Levelup.MessageHandlers
{
    public class NewLevelHandler : BaseMapObjectServerMapMessageHandler<NewRpgLevel>
    {
        protected override async Task InnerProcess(IRandom rand, MapMessagePackage pack, MapObject obj, NewRpgLevel message)
        {
            await Task.CompletedTask;
            obj.AddMessage(message);
        }
    }
}


