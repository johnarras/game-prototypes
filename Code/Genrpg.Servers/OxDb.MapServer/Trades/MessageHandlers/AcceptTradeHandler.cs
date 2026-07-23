using OxDb.MapServer.MapMessaging.MessageHandlers;
using OxDb.MapServer.Trades.Services;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Characters.PlayerData;
using OxDb.SharedGame.Trades.Messages;
using System.Threading.Tasks;

namespace OxDb.MapServer.Trades.MessageHandlers
{
    public class AcceptTradeHandler : BaseCharacterServerMapMessageHandler<AcceptTrade>
    {
        private ITradeService _tradeService = null;
        protected override async ValueTask InnerProcess(Character ch, AcceptTrade message)
        {
            await _tradeService.HandleAcceptTrade(ch, message);
        }
    }
}


