using OxDb.MapServer.MapMessaging.MessageHandlers;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.MapObjects.Entities;
using OxDb.SharedGame.Spells.Messages;
using System.Threading.Tasks;

namespace OxDb.MapServer.Spells.MessageHandlers
{
    public class OnUpdateEffectHandler : BaseMapObjectServerMapMessageHandler<OnUpdateEffect>
    {
        protected override async Task InnerProcess(IRandomContainer rand, MapObject obj, OnUpdateEffect message)
        {
            obj.AddMessage(message);
            await Task.CompletedTask;
        }
    }
}


