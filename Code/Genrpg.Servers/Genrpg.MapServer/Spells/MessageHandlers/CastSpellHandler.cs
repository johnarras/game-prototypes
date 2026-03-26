using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Spells.Messages;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Spells.MessageHandlers
{
    public class CastSpellHandler : BaseUnitServerMapMessageHandler<CastSpell>
    {
        protected override async Task InnerProcess(IRandom rand, MapMessagePackage pack, Unit unit, CastSpell message)
        {
            _spellService.FullTryStartCast(rand, unit, message.SpellId, message.TargetId);

        }
    }
}


