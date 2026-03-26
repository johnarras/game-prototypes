using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Spells.Messages;
using Genrpg.Shared.Utils;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Spells.MessageHandlers
{
    public class CombatTextHandler : BaseMapObjectServerMapMessageHandler<CombatText>
    {
        protected override async Task InnerProcess(IRandom rand, MapMessagePackage pack, MapObject obj, CombatText message)
        {
            obj.AddMessage(message);
        }
    }
}


