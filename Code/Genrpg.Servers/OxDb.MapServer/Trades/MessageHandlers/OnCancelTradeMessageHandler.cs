using OxDb.MapServer.MapMessaging.MessageHandlers;
using OxDb.MapServer.Trades.Services;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Characters.PlayerData;
using OxDb.SharedGame.Trades.Messages;
using System.Threading.Tasks;

namespace OxDb.MapServer.Trades.MessageHandlers
{
    public class OnCancelTradeHandler : BaseCharacterServerMapMessageHandler<OnCancelTrade>
    {
        private ITradeService _tradeService = null;
        protected override async Task InnerProcess(IRandomContainer rand, Character ch, OnCancelTrade message)
        {
            _tradeService.HandleOnCancelTrade(ch, message);
        }
    }
}


