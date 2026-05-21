using OxDb.MapServer.MapMessaging.MessageHandlers;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.MapObjects.Entities;
using OxDb.SharedGame.Spells.Messages;
using System.Threading.Tasks;

namespace OxDb.MapServer.Spells.MessageHandlers
{
    public class OnRemoveEffectHandler : BaseMapObjectServerMapMessageHandler<OnRemoveEffect>
    {
        protected override async Task InnerProcess(IRandomContainer rand, MapObject obj, OnRemoveEffect message)
        {
            obj.AddMessage(message);
            await Task.CompletedTask;
        }
    }
}


