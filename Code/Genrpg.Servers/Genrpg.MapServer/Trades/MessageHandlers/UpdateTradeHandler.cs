using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.MapServer.Trades.Services;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Trades.Messages;
using Genrpg.Shared.Utils;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Trades.MessageHandlers
{
    public class UpdateTradeHandler : BaseCharacterServerMapMessageHandler<UpdateTrade>
    {
        private ITradeService _tradeService = null;
        protected override async Task InnerProcess(IRandom rand, MapMessagePackage pack, Character ch, UpdateTrade message)
        {
            _tradeService.HandleUpdateTrade(ch, message);
        }
    }
}


