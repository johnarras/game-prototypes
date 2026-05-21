using OxDb.RequestServer.ClientUserRequests.RequestHandlers;
using OxDb.RequestServer.Core;
using OxDb.ServerCore.GameSettings.Services;
using OxDb.SharedCore.GameSettings.WebApi.UpdateGameSettings;
using OxDb.SharedGame.Characters.PlayerData;
using OxDb.SharedGame.Core.PlayerData;

namespace OxDb.RequestServer.GameSettings.RequestHandlers
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


