using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Spells.Messages;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Spells.MessageHandlers
{
    public class SendSpellHandler : BaseUnitServerMapMessageHandler<SendSpell>
    {
        protected override async Task InnerProcess(IRandom rand, MapMessagePackage pack, Unit unit, SendSpell message)
        {
            if (!_unitService.IsOkUnit(unit, true))
            {
                return;
            }
            _spellService.OnSendSpell(rand, unit, message);
        }
    }
}


