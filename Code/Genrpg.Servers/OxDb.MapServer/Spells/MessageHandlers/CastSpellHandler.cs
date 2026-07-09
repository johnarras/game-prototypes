using OxDb.MapServer.MapMessaging.MessageHandlers;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Spells.Messages;
using OxDb.SharedGame.Units.Entities;
using System.Threading.Tasks;

namespace OxDb.MapServer.Spells.MessageHandlers
{
    public class CastSpellHandler : BaseUnitServerMapMessageHandler<CastSpell>
    {
        protected override async ValueTask InnerProcess(Unit unit, CastSpell message)
        {
            _spellService.FullTryStartCast(unit, message.SpellId, message.TargetId);

        }
    }
}


