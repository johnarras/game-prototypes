using OxDb.MapServer.MapMessaging.MessageHandlers;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.MapObjects.Entities;
using OxDb.SharedGame.Spells.Messages;
using System.Threading.Tasks;

namespace OxDb.MapServer.Spells.MessageHandlers
{
    public class OnUpdateEffectHandler : BaseMapObjectServerMapMessageHandler<OnUpdateEffect>
    {
        protected override async ValueTask InnerProcess(MapObject obj, OnUpdateEffect message)
        {
            obj.AddMessage(message);
            await Task.CompletedTask;
        }
    }
}


