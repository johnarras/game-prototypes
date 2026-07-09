using OxDb.MapServer.MapMessaging.MessageHandlers;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Combat.Messages;
using OxDb.SharedGame.MapObjects.Entities;
using OxDb.SharedGame.Spells.Messages;
using System.Threading.Tasks;

namespace OxDb.MapServer.Spells.MessageHandlers
{
    public class InterruptCastHandler : BaseMapObjectServerMapMessageHandler<InterruptCast>
    {
        protected override async ValueTask InnerProcess(MapObject obj, InterruptCast message)
        {

            ICastTimeMessage castTimeMessage = obj.ActionMessage as ICastTimeMessage;
            if (castTimeMessage != null && castTimeMessage.CastingTime == 0)
            {
                return;
            }

            ICastMessage actorCastMessage = obj.ActionMessage as ICastMessage;

            if (actorCastMessage != null && _objectManager.GetObject(actorCastMessage.TargetId, out MapObject target))
            {
                ICastMessage targetCastMessage = target.OnActionMessage as ICastMessage;

                if (targetCastMessage != null &&
                    targetCastMessage.CasterId == actorCastMessage.CasterId &&
                    targetCastMessage.TargetId == actorCastMessage.TargetId)
                {
                    targetCastMessage.SetCancelled(true);
                }
            }

            if (obj.ActionMessage != null)
            {
                obj.ActionMessage.SetCancelled(true);
                obj.ActionMessage = null;
            }

            OnStopCast stop = obj.GetCachedMessage<OnStopCast>(true);
            stop.CasterId = obj.Id;

            _messageService.SendMessageNear(obj, stop);
        }
    }
}


