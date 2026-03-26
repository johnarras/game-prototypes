using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Spells.Casting;
using Genrpg.Shared.Spells.Messages;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.Targets.Messages;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Spells.MessageHandlers
{
    public class CastingSpellHandler : BaseUnitServerMapMessageHandler<CastingSpell>
    {
        private IStatService _statService = null;
        protected override async Task InnerProcess(IRandom rand, MapMessagePackage pack, Unit unit, CastingSpell message)
        {
            if (!_unitService.IsOkUnit(unit, true))
            {
                unit.ActionMessage = null;
                _spellService.SendStopCast(rand, unit);
                return;
            }

            if (message.Spell == null)
            {
                unit.ActionMessage = null;
                _spellService.SendStopCast(rand, unit);
                pack.SendError(unit, "Spell does not exist");
                return;
            }


            TryCastResult result = _spellService.TryCast(rand, unit, message.Spell.IdKey, message.TargetId, true);

            if (result.State != TryCastState.Ok)
            {
                pack.SendError(unit, result.StateText);
                if (result.State == TryCastState.TargetDead)
                {
                    unit.AddMessage(new OnTargetIsDead() { UnitId = message.TargetId });
                }
                _spellService.SendStopCast(rand, unit);
                return;
            }

            if (unit.ActionMessage != message)
            {
                pack.SendError(unit, "You aren't casting this spell");
                _spellService.SendStopCast(rand, unit);
                return;
            }

            _statService.Add(unit, result.Spell.PowerStatTypeId, UnitStatValOffsets.Curr, -result.Spell.GetCost(unit));

            // Send projectile to target.
            _spellService.SendSpell(rand, unit, result);
        }
    }
}


