using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.RpgLevels.Messages;
using Genrpg.Shared.Utils;

namespace Genrpg.MapServer.Levelup.MessageHandlers
{
    public class NewLevelHandler : BaseMapObjectServerMapMessageHandler<NewRpgLevel>
    {
        protected override void InnerProcess(IRandom rand, MapMessagePackage pack, MapObject obj, NewRpgLevel message)
        {
            obj.AddMessage(message);
        }
    }
}


