using Genrpg.RequestServer.Core;
using Genrpg.ServerShared.CloudComms.Constants;
using Genrpg.ServerShared.CloudComms.Servers.PlayerServer.Queues;
using Genrpg.ServerShared.DataStores;
using Genrpg.Shared.Accounts.PlayerData;
using Genrpg.Shared.Accounts.Settings;
using Genrpg.Shared.Accounts.WebApi.Login;
using Genrpg.Shared.Accounts.WebApi.NewVersions;
using Genrpg.Shared.DataStores.Categories.PlayerData.Units;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.Users.PlayerData;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Website.Messages.Error;
using System.ClientModel.Primitives;

namespace Genrpg.RequestServer.AuthRequests.GameAuthRequestHandlers
{
    public class GameAuthRequestHandler : BaseGameAuthRequestHandler<GameAuthRequest>
    {

        protected IGameData _gameData = null;
        protected override async Task HandleRequestInternal(WebContext context, GameAuthRequest request, CancellationToken token)
        {

            AuthSettings authSettings = _gameData.Get<AuthSettings>(null);

            Version requiredVersion = new Version(authSettings.MinClientVersion);
            Version clientVersion = new Version(request.ClientVersion);

            if (clientVersion < requiredVersion)
            {
                context.Responses.AddResponse(new NewVersionResponse() { MinNewClientVersion = authSettings.MinClientVersion });
                return;
            }

            AccountSessionData sessionData = await _serverRepoService.Load<AccountSessionData>(request.AccountId);

            if (sessionData == null)
            {
                context.Responses.AddResponse(new ErrorResponse() { Error = "Unknown account." });
                return;
            }

            if (sessionData.SessionId != request.SessionId)
            {
                context.Responses.AddResponse(new ErrorResponse() { Error = "Session Id must be refreshed." });
                return;
            }

            context.user = (await _serverRepoService.Search<User>(x => x.ProductAccountId == request.ProductAccountId && !x.Deleted)).FirstOrDefault();

            if (context.user == null)
            {
                context.user = new User()
                {
                    Id = sessionData.Id, // Not good idea if we want to have different users per account (for deletion)
                    ProductAccountId = request.ProductAccountId,
                    CreationDate = DateTime.UtcNow,
                };
            }

            context.user.SessionId = HashUtils.NewUUId();
            context.user.ClientVersion = request.ClientVersion;
            context.user.DataOverrides.GameDataCheckTime = request.ClientGameDataSaveTime;
            await _serverRepoService.Save(context.user);

            List<IUnitData> userData = await _loginPlayerDataService.LoadPlayerDataOnLogin(context, null);

            GameAuthResponse response = new GameAuthResponse()
            {
                User = _serializer.ConvertType<User, User>(context.user),
                CharacterStubs = await _playerDataService.LoadCharacterStubs(context.user.Id),
                MapStubs = _webServerService.GetMapStubs().Stubs,
                UserData = await _playerDataService.MapToClientDto(context.user, userData),
            };

            List<IGameSettingsLoader> loaders = _gameDataService.GetAllLoaders();

            _gameDataService.GetClientSettings(context.Responses, context.user, true);

            UpdatePublicUser(sessionData, context.user);

            _cloudCommsService.SendQueueMessage(CloudServerNames.Player, new LoginUser() { Id = context.user.Id, Name = "User" + context.user.Id });

            context.Responses.AddResponse(response);
        }

        private void UpdatePublicUser(AccountSessionData account, User user)
        {
            // Just always make new files and save them.

            PublicUser publicUser = new PublicUser() { Id = user.Id };
            publicUser.Name = account.ShareId;
            _serverRepoService.QueueSave(publicUser);

        }

    }
}
