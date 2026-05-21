using OxDb.MapServer.MapMessaging.MessageHandlers;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.MapObjects.Entities;
using OxDb.SharedGame.Spells.Messages;
using System.Threading.Tasks;

namespace OxDb.MapServer.Spells.MessageHandlers
{
    public class OnStartCastHandler : BaseMapObjectServerMapMessageHandler<OnStartCast>
    {
        protected override async Task InnerProcess(IRandomContainer rand, MapObject obj, OnStartCast message)
        {
            obj.AddMessage(message);
        }
    }
}


