using OxDb.MapServer.MapMessaging.MessageHandlers;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Spells.Messages;
using OxDb.SharedGame.Units.Entities;
using System.Threading.Tasks;

namespace OxDb.MapServer.Spells.MessageHandlers
{
    public class SendSpellHandler : BaseUnitServerMapMessageHandler<SendSpell>
    {
        protected override async Task InnerProcess(IRandomContainer rand, Unit unit, SendSpell message)
        {
            if (!_unitService.IsOkUnit(unit, true))
            {
                return;
            }
            _spellService.OnSendSpell(rand.Rand, unit, message);
        }
    }
}


