using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.Stats.Messages;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Stats.MessageHandlers
{
    public class RegenHandler : BaseUnitServerMapMessageHandler<Regen>
    {
        private IStatService _statService = null;

        protected override async Task InnerProcess(IRandom rand, MapMessagePackage pack, Unit unit, Regen message)
        {
            float regenSeconds = StatConstants.RegenTickSeconds;

            _statService.RegenerateTick(rand, unit, regenSeconds);

            if (unit.RegenMessage != null && !unit.RegenMessage.IsCancelled())
            {
                _messageService.SendMessage(unit, message, regenSeconds);
            }
        }
    }
}


