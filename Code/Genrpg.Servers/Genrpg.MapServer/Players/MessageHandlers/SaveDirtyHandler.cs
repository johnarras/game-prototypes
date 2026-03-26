using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.ServerShared.PlayerData.Services;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Players.Constants;
using Genrpg.Shared.Players.Messages;
using Genrpg.Shared.Utils;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Players.MessageHandlers
{
    public class SaveDirtyHandler : BaseCharacterServerMapMessageHandler<SaveDirty>
    {
        protected IPlayerDataService _playerDataService = null;
        protected override async Task InnerProcess(IRandom rand, MapMessagePackage pack, Character ch, SaveDirty message)
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


