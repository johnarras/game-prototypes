using OxDb.MapServer.MapMessaging.MessageHandlers;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.MapObjects.Entities;
using OxDb.SharedGame.Targets.Messages;
using System.Threading.Tasks;

namespace OxDb.MapServer.Units.MessageHandlers
{
    public class OnSetTargetHandler : BaseMapObjectServerMapMessageHandler<OnSetTarget>
    {
        protected override async ValueTask InnerProcess(MapObject obj, OnSetTarget message)
        {
            obj.AddMessage(message);
        }
    }
}


