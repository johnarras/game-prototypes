using Genrpg.ServerShared.GameSettings.Services;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.ClientUserRequests.RequestHandlers;
using Genrpg.Shared.GameSettings.WebApi.UpdateGameSettings;

namespace Genrpg.RequestServer.GameSettings.RequestHandlers
{
    public class UpdateGameSettingsHandler : BaseClientUserRequestHandler<UpdateGameSettingsRequest>
    {
        private IGameDataService _gameDataService = null;

        protected override async Task InnerHandleMessage(WebContext context, UpdateGameSettingsRequest request, CancellationToken token)
        {
            CoreCharacter coreCh = await _repoService.Load<CoreCharacter>(request.CharId);
            _gameDataService.GetClientSettings(context.Responses, context.user, false);
            
            await Task.CompletedTask;
        }
    }
}
