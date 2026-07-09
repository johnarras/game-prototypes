using OxDb.MapServer.MapMessaging.MessageHandlers;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.AI.Settings;
using OxDb.SharedGame.Units.Entities;
using System.Threading.Tasks;

namespace OxDb.MapServer.AI.MessageHandlers
{
    public class AIUpdateHandler : BaseUnitServerMapMessageHandler<AIUpdate>
    {
        protected override async ValueTask InnerProcess(Unit unit, AIUpdate message)
        {
            if (!_aiService.Update(unit))
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


