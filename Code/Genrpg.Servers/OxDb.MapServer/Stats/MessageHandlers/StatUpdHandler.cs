using OxDb.MapServer.MapMessaging.MessageHandlers;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.MapObjects.Entities;
using OxDb.SharedGame.Stats.Messages;
using System.Threading.Tasks;

namespace OxDb.MapServer.Stats.MessageHandlers
{
    public class StatUpdHandler : BaseMapObjectServerMapMessageHandler<StatUpd>
    {
        protected override async Task InnerProcess(IRandomContainer rand, MapObject obj, StatUpd message)
        {
            obj.AddMessage(message);
        }
    }
}


