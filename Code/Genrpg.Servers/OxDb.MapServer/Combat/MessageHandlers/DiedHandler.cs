using OxDb.MapServer.MapMessaging.MessageHandlers;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Combat.Messages;
using OxDb.SharedGame.Targets.Messages;
using OxDb.SharedGame.Units.Entities;
using System.Threading.Tasks;

namespace OxDb.MapServer.Combat.MessageHandlers
{
    public class DiedHandler : BaseUnitServerMapMessageHandler<Died>
    {
        protected override async ValueTask InnerProcess(Unit unit, Died message)
        {
            unit.AddMessage(message);
            unit.RemoveAttacker(message.UnitId);

            if (unit.TargetId == message.UnitId)
            {
                SetTarget setTarget = unit.GetCachedMessage<SetTarget>(true);
                setTarget.TargetId = "";

                _messageService.SendMessageNear(unit, setTarget);
            }

        }
    }
}


