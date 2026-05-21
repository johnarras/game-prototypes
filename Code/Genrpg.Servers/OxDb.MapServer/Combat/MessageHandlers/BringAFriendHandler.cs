using OxDb.MapServer.Combat.Messages;
using OxDb.MapServer.MapMessaging.MessageHandlers;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Units.Constants;
using OxDb.SharedGame.Units.Entities;
using System.Threading.Tasks;

namespace OxDb.MapServer.Combat.MessageHandlers
{
    public class BringAFriendHandler : BaseUnitServerMapMessageHandler<BringFriends>
    {
        protected override async Task InnerProcess(IRandomContainer rand, Unit unit, BringFriends message)
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
                _aiService.TargetMove(rand.Rand, unit, message.TargetId);
            }
            await Task.CompletedTask;
        }
    }
}


