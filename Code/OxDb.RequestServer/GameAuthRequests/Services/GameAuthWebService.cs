using OxDb.RequestServer.ClientUserRequests.Services;
using OxDb.RequestServer.Core;
using OxDb.RequestServer.GameAuthRequests.Constants;
using OxDb.RequestServer.Platform.Services;
using OxDb.RequestServer.PlayerData.Services;
using OxDb.RequestServer.Purchasing.Services;
using OxDb.ServerCore.CloudComms.Servers.PlayerServer.Queues;
using OxDb.ServerCore.CloudComms.Services;
using OxDb.ServerCore.Config;
using OxDb.ServerCore.Constants;
using OxDb.ServerCore.Crypto.Services;
using OxDb.ServerCore.GameSettings.Services;
using OxDb.ServerCore.Platform.Constants;
using OxDb.ServerCore.Platform.WebApi;
using OxDb.ServerGame.PlayerData.Services;
using OxDb.SharedCore.Config.Constants;
using OxDb.SharedCore.Core.Constants;
using OxDb.SharedCore.DataStores.Interfaces;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedCore.Website.Requests.Core;
using OxDb.SharedCore.Website.Responses.Errors;
using OxDb.SharedGame.Auth.Settings;
using OxDb.SharedGame.Core.PlayerData;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Units;
using OxDb.SharedGame.GameAuth.Interfaces;
using OxDb.SharedGame.GameAuth.WebApi.Auth;
using OxDb.SharedGame.GameAuth.WebApi.NewVersions;
using OxDb.SharedGame.GameAuth.WebApi.RefreshToken;
using OxDb.SharedGame.Purchasing.PlayerData;

namespace OxDb.RequestServer.GameAuthRequests.Services
{
    public class GameAuthWebService : IGameAuthWebService
    {
        private IRepositoryService _repoService = null;
        private IGameData _gameData = null;
        protected IPlayerDataService _playerDataService = null!;
        protected ILoadPlayerDataService _loginPlayerDataService = null!;
        protected IServerConfig _serverConfig = null!;
        protected IServerGameDataService _gameDataService = null!;
        protected ICloudCommsService _cloudCommsService = null!;
        protected ICryptoService _cryptoService = null!;
        private IServerPurchasingService _purchasingService = null;
        private ILogService _logService = null;
        private IGameToPlatformAuthService _platformService = null;
        private IGameClientRequestService _gameClientRequestService = null;

        private ReadOnlyString _tokenSecret = null;

        public async Task Initialize(CancellationToken token)
        {
            _tokenSecret = new ReadOnlyString(_serverConfig.GetConfigVal(AppConfigKeys.TokenSecret));
            await Task.CompletedTask;
        }

        protected void ShowErrorMessage(WebContext context, EGameAuthStates state)
        {
            context.ClearResponses();
            context.AddResponse(new ErrorResponse() { Error = StrUtils.SplitOnCapitalLetters(state.ToString()) });
        }

        public async Task HandleGameAuthRequest(WebContext context, WebServerRequestSet requestSet, CancellationToken token)
        {

            try
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

                long dataBits = 0;
                // Must explicitly load for GameAccount so a stub doc isn't created. 
                GameAccount gameAccount = await _repoService.Load<GameAccount>(request.GameUserId);

                bool didCreateAccount = false;
                if (gameAccount == null)
                {
                    if (request.DataBits != 0)
                    {
                        ShowErrorMessage(context, EGameAuthStates.MissingExistingAccount);
                        return;
                    }
                    gameAccount = new GameAccount()
                    {
                        Id = request.GameUserId,
                        AccountId = request.AccountId,
                        CreationDate = DateTime.UtcNow,
                    };
                    didCreateAccount = true;
                }
                dataBits = gameAccount.DataBits;

                ProductToPlatformAuthRequest platformAuthRequest = new ProductToPlatformAuthRequest()
                {
                    AccountId = request.AccountId,
                    ProductUserId = request.GameUserId,
                    ProductId = request.ProductId,
                    SessionId = request.SessionId,
                    DataBits = dataBits,
                };

                ProductToPlatformAuthResponse authResponse = await _platformService.CheckPlatformAuth(platformAuthRequest);

                if (authResponse.State != EPlatformAuthStates.Success)
                {
                    context.AddResponse(new ErrorResponse() { Error = StrUtils.SplitOnCapitalLetters(authResponse.State.ToString()) });
                    return;
                }

                gameAccount.GameUserId = request.GameUserId;

                // Must explicitly do this on game auth in case this doc doesn't exist.
                context.SetGameUserId(request.GameUserId);
                context.Set(gameAccount);

                if (gameAccount.AccountId != request.AccountId)
                {
                    ShowErrorMessage(context, EGameAuthStates.IncorrectGameUserId);
                    return;
                }

                if (gameAccount.Deleted)
                {
                    ShowErrorMessage(context, EGameAuthStates.NoUserWithThatId);
                    return;
                }

                gameAccount.ClientVersion = request.ClientVersion;
                gameAccount.ClientPlatformName = request.ClientPlatformName;
                gameAccount.ShareId = request.ShareId;

                CoreData coreData = await context.GetAsync<CoreData>();
                coreData.AB.CheckTime = request.ClientGameDataSaveTime;
                coreData.Client = gameAccount.ClientVersion;

                List<IUnitData> allUserData = await _loginPlayerDataService.LoadPlayerDataOnLogin(context, null, gameAccount);

                await UpdatePublicUser(gameAccount);

                _cloudCommsService.SendQueueMessage(ServerNames.Player, new LoginUser() { Id = gameAccount.GameUserId, Name = "User" + context.GameUserId });

                await _purchasingService.RetryFailedValidation(context, token);

                PlayerStoreOfferData offerData = await _purchasingService.GetCurrentStores(context, coreData, true, token);

                GameAuthResponse response = new GameAuthResponse()
                {
                    UserData = await _playerDataService.MapToClientDto(coreData, allUserData),
                    OfferData = offerData,
                    GameUserId = context.GameUserId,
                    ServerEnv = _serverConfig.Env,
                    ServerName = _serverConfig.GameComponent,
                    ServerVersion = _serverConfig.ServerVersion,
                    DidCreateAccount = didCreateAccount,
                };

                await SetSessionData(context, response, gameAccount);

                // Don't do this for anything other than the MMO game.
                if (request.GameName == EGameModes.MMO.ToString())
                {
                    response.CharacterStubs = await _playerDataService.LoadCharacterStubs(context.GameUserId);
                    response.MapStubs = _gameClientRequestService.GetMapStubs().Stubs;
                }

                context.AddFront(response);
                // This should be a transaction that fails or succeeds.
                bool didSave = await context.SaveAllOneTime();

                if (!didSave)
                {
                    ShowErrorMessage(context, EGameAuthStates.FailedToPersistData);
                    return;
                }
            }
            catch (Exception ex)
            {
                _logService.Exception(ex, "Login");
                context.ClearResponses();
                context.AddResponse(new ErrorResponse() { Error = "Please try again in a few minutes.\n" + ex.Message });
            }
        }

        private async Task SetSessionData(WebContext context, IGameSessionState sessionState, GameAccount acct)
        {
            // Token has 3 pieces: UserId, RandString, EndTimestamp

            // Concatenate with . and then hash using secret, then concatenate that hash with _ and that's the token.
            // Used to avoid looking up stuff in a cache.

            string userName = acct.GameUserId;
            string sessionId = HashUtils.NewGuid().Replace("-", "");
            long endTicks = DateTime.UtcNow.AddMinutes(GameAuthConstants.SessionTokenTtlMinutes).Ticks;

            long existingDocuments = acct.DataBits;

            string tokenData = userName + "." + sessionId + "." + endTicks + "." + existingDocuments;

            string secret = _tokenSecret.GetString();

            string hash = HashUtils.QuickHash(tokenData + "." + secret);

            acct.RefreshToken = HashUtils.NewGuid().ToString();
            sessionState.SelfContainedToken = tokenData + "_" + hash;
            sessionState.SessionId = sessionId;
            sessionState.RefreshToken = acct.RefreshToken;
            acct.SessionToken = sessionState.SelfContainedToken;

            await Task.CompletedTask;

        }
        private async Task UpdatePublicUser(GameAccount gameAccount)
        {
            // Just always make new files and save them.

            PublicUser publicUser = new PublicUser() { Id = gameAccount.GameUserId };
            publicUser.Name = gameAccount.ShareId;
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
            response.ServerVersion = _serverConfig.ServerVersion;
            response.ServerName = _serverConfig.GameComponent;
            response.ServerEnv = _serverConfig.Env;

            context.SetGameUserId(request.GameUserId);
            context.AddResponse(response);

            await _repoService.Save(gameAcct);

        }
    }
}



