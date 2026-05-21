using OxDb.MapServer.MapMessaging.MessageHandlers;
using OxDb.MapServer.Trades.Services;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Characters.PlayerData;
using OxDb.SharedGame.Trades.Messages;
using System.Threading.Tasks;

namespace OxDb.MapServer.Trades.MessageHandlers
{
    public class CancelTradeHandler : BaseCharacterServerMapMessageHandler<CancelTrade>
    {
        private ITradeService _tradeService = null;
        protected override async Task InnerProcess(IRandomContainer rand, Character ch, CancelTrade message)
        {
            _tradeService.HandleCancelTrade(ch, message);
        }
    }
}


