using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.GameAuthRequests.Constants;
using Genrpg.RequestServer.PlayerData.Services;
using Genrpg.RequestServer.Purchasing.Services;
using Genrpg.RequestServer.Services.WebServer;
using Genrpg.ServerShared.Accounts.Services;
using Genrpg.ServerShared.CloudComms.Constants;
using Genrpg.ServerShared.CloudComms.Servers.PlayerServer.Queues;
using Genrpg.ServerShared.CloudComms.Services;
using Genrpg.ServerShared.Config;
using Genrpg.ServerShared.Crypto.Services;
using Genrpg.ServerShared.GameSettings.Services;
using Genrpg.ServerShared.PlayerData.Services;
using Genrpg.Shared.Accounts.PlayerData;
using Genrpg.Shared.Accounts.Settings;
using Genrpg.Shared.Config.Constants;
using Genrpg.Shared.Core.Constants;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.DataStores.Categories.PlayerData.Units;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.GameAuth.Interfaces;
using Genrpg.Shared.GameAuth.WebApi.Auth;
using Genrpg.Shared.GameAuth.WebApi.NewVersions;
using Genrpg.Shared.GameAuth.WebApi.RefreshToken;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.Purchasing.PlayerData;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Website.Messages;
using Genrpg.Shared.Website.Messages.Error;

namespace Genrpg.RequestServer.Services.GameAuth
{
    public class GameAuthWebService : IGameAuthWebService
    {
        private IWebServerService _webServerService = null;
        private IRepositoryService _repoService = null;
        private IGameData _gameData = null;
        protected IPlayerDataService _playerDataService = null!;
        protected ILoadPlayerDataService _loginPlayerDataService = null!;
        protected IServerConfig _config = null!;
        protected IServerGameDataService _gameDataService = null!;
        protected ICloudCommsService _cloudCommsService = null!;
        protected IAccountService _accountService = null!;
        protected ICryptoService _cryptoService = null!;
        private IServerPurchasingService _purchasingService = null;

        public async Task HandleGameAuthRequest(WebContext context, WebServerRequestSet requestSet, CancellationToken token)
        {

            GameAuthRequest request = (GameAuthRequest)requestSet.Requests.FirstOrDefault(x => x.GetType() == typeof(GameAuthRequest));
            AuthSettings authSettings = _gameData.Get<AuthSettings>(null);

            Version requiredVersion = new Version(authSettings.MinClientVersion);
            Version clientVersion = new Version(request.ClientVersion);

            if (clientVersion < requiredVersion)
            {
                context.AddResponse(new NewVersionResponse() { MinNewClientVersion = authSettings.MinClientVersion });
                return;
            }

            AccountSessionData accountSessionData = await _repoService.Load<AccountSessionData>(request.AccountId);

            if (accountSessionData == null)
            {
                context.AddResponse(new ErrorResponse() { Error = "Unknown account." });
                return;
            }

            if (accountSessionData.SessionToken != request.SessionToken)
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
                };
            }

            gameAcct.GameUserId = request.GameUserId;

            // Must explicitly do this on game auth in case this doc doesn't exist.
            context.SetGameUserId(request.GameUserId);
            context.Set(gameAcct);

            if (gameAcct.AccountId != request.AccountId ||
                gameAcct.Deleted)
            {
                context.AddResponse(new ErrorResponse() { Error = "Internal account error." });
                return;
            }

            gameAcct.ClientVersion = request.ClientVersion;
            gameAcct.ClientPlatformName = request.ClientPlatformName;

            context.core = await context.GetAsync<CoreData>();
            context.core.DataOverrides.GameDataCheckTime = request.ClientGameDataSaveTime;
            context.core.ClientVersion = gameAcct.ClientVersion;

            List<IUnitData> allUserData = await _loginPlayerDataService.LoadPlayerDataOnLogin(context, null);

            await UpdatePublicUser(accountSessionData, gameAcct);

            _cloudCommsService.SendQueueMessage(CloudServerNames.Player, new LoginUser() { Id = gameAcct.GameUserId, Name = "User" + context.GameUserId });

            await _purchasingService.RetryFailedValidation(context, token);

            PlayerStoreOfferData offerData = await _purchasingService.GetCurrentStores(context, context.core, true, token);

            GameAuthResponse response = new GameAuthResponse()
            {
                UserData = await _playerDataService.MapToClientDto(context.core, allUserData),
                OfferData = offerData,
                GameUserId = context.GameUserId,
            };

            await SetSessionData(context, response, gameAcct);

            // Don't do this for anything other than the MMO game.
            if (request.GameName == EGameModes.MMO.ToString())
            {
                response.CharacterStubs = await _playerDataService.LoadCharacterStubs(context.GameUserId);
                response.MapStubs = _webServerService.GetMapStubs().Stubs;
            }

            context.AddFront(response);
            await context.SaveAllOneTime();
        }

        private async Task SetSessionData(WebContext context, IGameSessionState sessionState, GameAccount acct)
        {
            // Token has 3 pieces: UserId, RandString, EndTimestamp

            // Concatenate with . and then hash using secret, then concatenate that hash with _ and that's the token.
            // Used to avoid looking up stuff in a cache.

            string userName = acct.GameUserId;
            string tempRand = HashUtils.NewGuid().Replace("-", "");
            long endTicks = DateTime.UtcNow.AddMinutes(GameAuthConstants.SessionTokenTtlMinutes).Ticks;

            string tokenData = userName + "." + tempRand + "." + endTicks;

            string secret = _config.GetSecret(AppConfigKeys.TokenSecret);

            string hash = HashUtils.QuickHash(tokenData + "." + secret);

            acct.RefreshToken = Guid.NewGuid().ToString();
            sessionState.SessionToken = tokenData + "_" + hash;
            sessionState.RefreshToken = acct.RefreshToken;
            acct.SessionToken = sessionState.SessionToken;

            await Task.CompletedTask;

        }
        private async Task UpdatePublicUser(AccountSessionData account, GameAccount gameAccount)
        {
            // Just always make new files and save them.

            PublicUser publicUser = new PublicUser() { Id = gameAccount.GameUserId };
            publicUser.Name = account.ShareId;
            await _repoService.Save(publicUser);
        }

        public async Task HandleRefreshTokenRequest(WebContext context, WebServerRequestSet requestSet, CancellationToken token)
        {
            RefreshGameTokenRequest request = (RefreshGameTokenRequest)requestSet.Requests.FirstOrDefault(x => x.GetType() == typeof(RefreshGameTokenRequest));

            GameAccount gameAcct = await _repoService.Load<GameAccount>(request.GameUserId);

            RefreshGameTokenResponse response = new RefreshGameTokenResponse();

            if (gameAcct == null)
            {
                response.ErrorMessage = "Account Does Not Exist";
                context.AddResponse(response);
                return;
            }

            if (gameAcct.RefreshToken != request.RefreshToken)
            {
                response.ErrorMessage = "Refresh Token is Wrong";
                context.AddResponse(response);
                return;
            }

            await SetSessionData(context, response, gameAcct);

            response.Success = true;
            response.GameUserId = request.GameUserId;

            context.SetGameUserId(request.GameUserId);
            context.AddResponse(response);

            await _repoService.Save(gameAcct);

        }
    }
}



