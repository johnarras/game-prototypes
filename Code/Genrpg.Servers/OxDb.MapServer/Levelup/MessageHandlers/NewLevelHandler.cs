using OxDb.MapServer.MapMessaging.MessageHandlers;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.MapObjects.Entities;
using OxDb.SharedGame.RpgLevels.Messages;
using System.Threading.Tasks;

namespace OxDb.MapServer.Levelup.MessageHandlers
{
    public class NewLevelHandler : BaseMapObjectServerMapMessageHandler<NewRpgLevel>
    {
        protected override async ValueTask InnerProcess(MapObject obj, NewRpgLevel message)
        {
            await Task.CompletedTask;
            obj.AddMessage(message);
        }
    }
}


