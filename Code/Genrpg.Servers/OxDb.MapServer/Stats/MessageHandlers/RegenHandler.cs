using OxDb.MapServer.MapMessaging.MessageHandlers;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Stats.Constants;
using OxDb.SharedGame.Stats.Messages;
using OxDb.SharedGame.Units.Entities;
using System.Threading.Tasks;

namespace OxDb.MapServer.Stats.MessageHandlers
{
    public class RegenHandler : BaseUnitServerMapMessageHandler<Regen>
    {
        private IStatService _statService = null;

        protected override async Task InnerProcess(IRandomContainer rand, Unit unit, Regen message)
        {
            float regenSeconds = StatConstants.RegenTickSeconds;

            _statService.RegenerateTick(rand.Rand, unit, regenSeconds);

            if (unit.RegenMessage != null && !unit.RegenMessage.IsCancelled())
            {
                _messageService.SendMessage(unit, message, regenSeconds);
            }
        }
    }
}


