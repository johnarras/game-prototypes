using OxDb.MapServer.MapMessaging.MessageHandlers;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.MapObjects.Entities;
using OxDb.SharedGame.MapObjects.Messages;
using System.Threading.Tasks;

namespace OxDb.MapServer.MapObjects.Messages
{
    public class OnGetMapObjectStatusHandler : BaseMapObjectServerMapMessageHandler<OnGetMapObjectStatus>
    {
        protected override async ValueTask InnerProcess(MapObject obj, OnGetMapObjectStatus message)
        {
            obj.AddMessage(message);
        }
    }
}


