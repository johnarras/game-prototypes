using Genrpg.MapServer.Combat.Messages;
using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Units.Constants;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Combat.MessageHandlers
{
    public class BringAFriendHandler : BaseUnitServerMapMessageHandler<BringFriends>
    {
        protected override async Task InnerProcess(IRandom rand, MapMessagePackage pack, Unit unit, BringFriends message)
        {
            if (unit.IsPlayer() || unit.HasFlag(UnitFlags.IsDead | UnitFlags.Evading))
            {
                return;
            }

            if (unit.FactionTypeId != message.BringerFactionId)
            {
                return;
            }

            if (unit.HasTarget())
            {
                unit.AddAttacker(message.TargetId, null);
            }
            else
            {
                _aiService.TargetMove(rand, unit, message.TargetId);
            }
            await Task.CompletedTask;
        }
    }
}


