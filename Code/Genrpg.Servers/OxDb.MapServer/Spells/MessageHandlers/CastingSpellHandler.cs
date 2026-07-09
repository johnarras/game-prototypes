using OxDb.MapServer.MapMessaging.MessageHandlers;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Spells.Casting;
using OxDb.SharedGame.Spells.Messages;
using OxDb.SharedGame.Stats.Constants;
using OxDb.SharedGame.Targets.Messages;
using OxDb.SharedGame.Units.Entities;
using System.Threading.Tasks;

namespace OxDb.MapServer.Spells.MessageHandlers
{
    public class CastingSpellHandler : BaseUnitServerMapMessageHandler<CastingSpell>
    {
        private IStatService _statService = null;
        protected override async ValueTask InnerProcess(Unit unit, CastingSpell message)
        {
            if (!_unitService.IsOkUnit(unit, true))
            {
                unit.ActionMessage = null;
                _spellService.SendStopCast(unit);
                return;
            }

            if (message.Spell == null)
            {
                unit.ActionMessage = null;
                _spellService.SendStopCast(unit);
                unit.SendError("Spell does not exist");
                return;
            }


            TryCastResult result = _spellService.TryCast(unit, message.Spell.IdKey, message.TargetId, true);

            if (result.State != TryCastState.Ok)
            {
                unit.SendError(result.StateText);
                if (result.State == TryCastState.TargetDead)
                {
                    unit.AddMessage(new OnTargetIsDead() { UnitId = message.TargetId });
                }
                _spellService.SendStopCast(unit);
                return;
            }

            if (unit.ActionMessage != message)
            {
                unit.SendError("You aren't casting this spell");
                _spellService.SendStopCast(unit);
                return;
            }

            _statService.Add(unit, result.Spell.PowerStatTypeId, UnitStatValOffsets.Curr, -result.Spell.GetCost(unit));

            // Send projectile to target.
            _spellService.SendSpell(unit, result);
        }
    }
}


