using Genrpg.MapServer.Combat.Messages;
using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Combat.MessageHandlers
{
    public class AddAttackerHandler : BaseUnitServerMapMessageHandler<AddAttacker>
    {
        protected override async Task InnerProcess(IRandom rand, MapMessagePackage pack, Unit unit, AddAttacker message)
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


