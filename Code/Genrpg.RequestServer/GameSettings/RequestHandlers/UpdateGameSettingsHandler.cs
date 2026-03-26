using Genrpg.RequestServer.ClientUserRequests.RequestHandlers;
using Genrpg.RequestServer.Core;
using Genrpg.ServerShared.GameSettings.Services;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.GameSettings.WebApi.UpdateGameSettings;

namespace Genrpg.RequestServer.GameSettings.RequestHandlers
{
    public class UpdateGameSettingsHandler : BaseClientUserRequestHandler<UpdateGameSettingsRequest>
    {
        private IServerGameDataService _gameDataService = null;

        protected override async Task InnerHandleMessage(WebContext context, UpdateGameSettingsRequest request, CancellationToken token)
        {
            CoreCharacter coreCh = await _repoService.Load<CoreCharacter>(request.CharId);
            context.AddResponseRange(_gameDataService.GetClientSettings(await context.GetAsync<CoreData>(), false));

            await Task.CompletedTask;
        }
    }
}


