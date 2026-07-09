using OxDb.MapServer.MapMessaging.MessageHandlers;
using OxDb.MapServer.Trades.Services;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Characters.PlayerData;
using OxDb.SharedGame.Trades.Messages;
using System.Threading.Tasks;

namespace OxDb.MapServer.Trades.MessageHandlers
{
    public class StartTradeHandler : BaseCharacterServerMapMessageHandler<StartTrade>
    {
        private ITradeService _tradeService = null;
        protected override async ValueTask InnerProcess(Character ch, StartTrade message)
        {
            _tradeService.HandleStartTrade(ch, message);
        }
    }
}


