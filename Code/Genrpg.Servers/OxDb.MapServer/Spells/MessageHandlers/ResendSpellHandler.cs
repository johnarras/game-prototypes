using OxDb.MapServer.MapMessaging.MessageHandlers;
using OxDb.MapServer.Units.Services;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.MapObjects.Entities;
using OxDb.SharedGame.Spells.Messages;
using OxDb.SharedGame.Spells.PlayerData.Spells;
using OxDb.SharedGame.Spells.Utils;
using OxDb.SharedGame.Units.Entities;
using System.Threading.Tasks;

namespace OxDb.MapServer.Spells.MessageHandlers
{
    public class ResendSpellHandler : BaseMapObjectServerMapMessageHandler<ResendSpell>
    {
        protected IServerUnitService _unitService = null;
        protected override async ValueTask InnerProcess(MapObject obj, ResendSpell message)
        {
            if (message.ShotsLeft < 1)
            {
                return;
            }

            if (!_objectManager.GetUnit(message.SpellMessage.CasterId, out Unit caster) ||
                !_unitService.IsOkUnit(caster, true))
            {
                return;
            }

            if (!_objectManager.GetUnit(message.TargetId, out Unit target) ||
                !_unitService.IsOkUnit(target, true))
            {
                return;
            }

            _spellService.ResendSpell(caster, target, message.SpellMessage);

            message.ShotsLeft--;

            if (message.ShotsLeft > 0)
            {

                _messageService.SendMessage(caster, message, SpellUtils.GetResendDelay(message.SpellMessage.Spell.HasFlag(SpellFlags.InstantHit)));
            }
            await Task.CompletedTask;
        }
    }
}


