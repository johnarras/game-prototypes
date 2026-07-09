using OxDb.MapServer.MapMessaging.MessageHandlers;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Targets.Messages;
using OxDb.SharedGame.Units.Entities;
using System.Threading.Tasks;

namespace OxDb.MapServer.Units.MessageHandlers
{
    public class SetTargetHandler : BaseUnitServerMapMessageHandler<SetTarget>
    {
        protected override async ValueTask InnerProcess(Unit unit, SetTarget message)
        {

            string targetId = null;
            if (!string.IsNullOrEmpty(message.TargetId))
            {
                if (_objectManager.GetUnit(message.TargetId, out Unit targetObject))
                {
                    targetId = message.TargetId;
                }
            }
            unit.TargetId = targetId;

            OnSetTarget onSet = unit.GetCachedMessage<OnSetTarget>(true);
            onSet.CasterId = unit.Id;
            onSet.TargetId = targetId;

            _messageService.SendMessageNear(unit, onSet);
        }
    }
}


