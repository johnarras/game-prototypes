using OxDb.MapServer.MapMessaging.MessageHandlers;
using OxDb.ServerGame.PlayerData.Services;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Characters.PlayerData;
using OxDb.SharedGame.Players.Constants;
using OxDb.SharedGame.Players.Messages;
using System.Threading.Tasks;

namespace OxDb.MapServer.Players.MessageHandlers
{
    public class SaveDirtyHandler : BaseCharacterServerMapMessageHandler<SaveDirty>
    {
        protected IPlayerDataService _playerDataService = null;
        protected override async Task InnerProcess(IRandomContainer rand, Character ch, SaveDirty message)
        {
            await Task.CompletedTask;
            _playerDataService.SavePlayerData(ch);

            if (!message.IsCancelled())
            {
                _messageService.SendMessage(ch, message, PlayerConstants.SaveDelay);
            }
        }
    }
}


