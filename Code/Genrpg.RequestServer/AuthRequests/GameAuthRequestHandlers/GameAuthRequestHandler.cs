using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.Purchasing.Services;
using Genrpg.ServerShared.CloudComms.Constants;
using Genrpg.ServerShared.CloudComms.Servers.PlayerServer.Queues;
using Genrpg.Shared.Accounts.PlayerData;
using Genrpg.Shared.Accounts.Settings;
using Genrpg.Shared.Accounts.WebApi.Login;
using Genrpg.Shared.Accounts.WebApi.NewVersions;
using Genrpg.Shared.Core.Constants;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.DataStores.Categories.PlayerData.Units;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.Purchasing.PlayerData;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Website.Messages.Error;

namespace Genrpg.RequestServer.AuthRequests.GameAuthRequestHandlers
{
    public class GameAuthRequestHandler : BaseGameAuthRequestHandler<GameAuthRequest>
    {

        protected IGameData _gameData = null;
        protected IServerPurchasingService _purchasingService = null;

        protected override async Task HandleRequestInternal(WebContext context, GameAuthRequest request, CancellationToken token)
        {

            AuthSettings authSettings = _gameData.Get<AuthSettings>(null);

            Version requiredVersion = new Version(authSettings.MinClientVersion);
            Version clientVersion = new Version(request.ClientVersion);

            if (clientVersion < requiredVersion)
            {
                context.AddResponse(new NewVersionResponse() { MinNewClientVersion = authSettings.MinClientVersion });
                return;
            }

            AccountSessionData sessionData = await _repoService.Load<AccountSessionData>(request.AccountId);

            if (sessionData == null)
            {
                context.AddResponse(new ErrorResponse() { Error = "Unknown account." });
                return;
            }

            if (sessionData.SessionId != request.SessionId)
            {
                context.AddResponse(new ErrorResponse() { Error = "Session Id must be refreshed." });
                return;
            }

            // Must explicitly load for GameAccount so a stub doc isn't created. 
            GameAccount gameAcct = await _repoService.Load<GameAccount>(request.GameUserId);

            if (gameAcct == null)
            {
                gameAcct = new GameAccount()
                {
                    Id = request.GameUserId,
                    AccountId = request.AccountId,
                    CreationDate = DateTime.UtcNow,
                    GameUserId = request.GameUserId,
                };
            }


            // Must explicitly do this on game auth in case this doc doesn't exist.
            context.SetAccount(gameAcct);

            if (gameAcct.AccountId != request.AccountId ||
                gameAcct.Deleted)
            {
                context.AddResponse(new ErrorResponse() { Error = "Internal account error." });
                return;
            }

            gameAcct.SessionId = HashUtils.NewUUId();
            gameAcct.ClientVersion = request.ClientVersion;
            gameAcct.ClientPlatformName = request.ClientPlatformName;

            context.core = await context.GetAsync<CoreData>();
            context.core.DataOverrides.GameDataCheckTime = request.ClientGameDataSaveTime;
            context.core.ClientVersion = gameAcct.ClientVersion;

            List<IUnitData> allUserData = await _loginPlayerDataService.LoadPlayerDataOnLogin(context, null);

            List<IGameSettingsLoader> loaders = _gameDataService.GetAllLoaders();

            context.AddResponseRange(_gameDataService.GetClientSettings(context.core, true));

            await UpdatePublicUser(sessionData, gameAcct);

            _cloudCommsService.SendQueueMessage(CloudServerNames.Player, new LoginUser() { Id = gameAcct.Id, Name = "User" + context.GameUserId });

            await _purchasingService.RetryFailedValidation(context, token);

            PlayerStoreOfferData offerData = await _purchasingService.GetCurrentStores(context, context.core, true, token);

            GameAuthResponse response = new GameAuthResponse()
            {
                GameUserId = gameAcct.GameUserId,
                SessionId = gameAcct.SessionId,
                UserData = await _playerDataService.MapToClientDto(context.core, allUserData),
                OfferData = offerData,
            };

            // Don't do this for anything other than the MMO game.
            if (request.GameName == EGameModes.MMO.ToString())
            {
                response.CharacterStubs = await _playerDataService.LoadCharacterStubs(context.GameUserId);
                response.MapStubs = _webServerService.GetMapStubs().Stubs;
            }

            context.AddFront(response);
        }

        private async Task UpdatePublicUser(AccountSessionData account, GameAccount gameAccount)
        {
            // Just always make new files and save them.

            PublicUser publicUser = new PublicUser() { Id = gameAccount.GameUserId };
            publicUser.Name = account.ShareId;
            await _repoService.Save(publicUser);

        }
    }
}


