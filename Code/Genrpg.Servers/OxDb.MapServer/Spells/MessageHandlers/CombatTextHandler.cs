using OxDb.MapServer.MapMessaging.MessageHandlers;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.MapObjects.Entities;
using OxDb.SharedGame.Spells.Messages;
using System.Threading.Tasks;

namespace OxDb.MapServer.Spells.MessageHandlers
{
    public class CombatTextHandler : BaseMapObjectServerMapMessageHandler<CombatText>
    {
        protected override async ValueTask InnerProcess(MapObject obj, CombatText message)
        {
            obj.AddMessage(message);
        }
    }
}


