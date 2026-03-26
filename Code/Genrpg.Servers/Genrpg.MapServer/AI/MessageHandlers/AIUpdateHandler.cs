using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.Shared.AI.Settings;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;
using System.Threading.Tasks;

namespace Genrpg.MapServer.AI.MessageHandlers
{
    public class AIUpdateHandler : BaseUnitServerMapMessageHandler<AIUpdate>
    {
        protected override async Task InnerProcess(IRandom rand, MapMessagePackage pack, Unit unit, AIUpdate message)
        {
            if (!_aiService.Update(rand, unit))
            {
                return;
            }

            if (_unitService.IsOkUnit(unit, false))
            {
                float delayTime = _gameData.Get<AISettings>(unit).UpdateSeconds;
                _messageService.SendMessage(unit, message, delayTime);
            }
            await Task.CompletedTask;
        }
    }
}


