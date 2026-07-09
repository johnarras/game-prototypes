using OxDb.MapServer.Combat.Messages;
using OxDb.MapServer.MapMessaging.MessageHandlers;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Units.Entities;
using System.Threading.Tasks;

namespace OxDb.MapServer.Combat.MessageHandlers
{
    public class AddAttackerHandler : BaseUnitServerMapMessageHandler<AddAttacker>
    {
        protected override async ValueTask InnerProcess(Unit unit, AddAttacker message)
        {
            if (!_unitService.IsOkUnit(unit, false))
            {
                return;
            }

            unit.AddAttacker(message.AttackerId, null);
            await Task.CompletedTask;
        }
    }
}


