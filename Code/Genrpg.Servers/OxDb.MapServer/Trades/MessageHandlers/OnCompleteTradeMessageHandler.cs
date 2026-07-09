using OxDb.MapServer.MapMessaging.MessageHandlers;
using OxDb.MapServer.Trades.Services;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Characters.PlayerData;
using OxDb.SharedGame.Trades.Messages;
using System.Threading.Tasks;

namespace OxDb.MapServer.Trades.MessageHandlers
{
    public class OnCompleteTradeHandler : BaseCharacterServerMapMessageHandler<OnCompleteTrade>
    {
        private ITradeService _tradeService = null;
        protected override async ValueTask InnerProcess(Character ch, OnCompleteTrade message)
        {
            _tradeService.HandleOnCompleteTrade(ch, message);
        }
    }
}


