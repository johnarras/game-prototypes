using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.MapServer.Trades.Services;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Trades.Messages;
using Genrpg.Shared.Utils;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Trades.MessageHandlers
{
    public class CancelTradeHandler : BaseCharacterServerMapMessageHandler<CancelTrade>
    {
        private ITradeService _tradeService = null;
        protected override async Task InnerProcess(IRandom rand, MapMessagePackage pack, Character ch, CancelTrade message)
        {
            _tradeService.HandleCancelTrade(ch, message);
        }
    }
}


