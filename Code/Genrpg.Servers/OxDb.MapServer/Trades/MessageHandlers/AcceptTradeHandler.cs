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
        protected override async Task InnerProcess(IRandomContainer rand, Character ch, AcceptTrade message)
        {
            _tradeService.HandleAcceptTrade(ch, message, rand.Rand);
        }
    }
}


