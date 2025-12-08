using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.Purchasing.Services;
using Genrpg.ServerShared.CloudComms.Constants;
using Genrpg.ServerShared.CloudComms.Servers.PlayerServer.Queues;
using Genrpg.Shared.Accounts.PlayerData;
using Genrpg.Shared.Accounts.Settings;
using Genrpg.Shared.Accounts.WebApi.Login;
using Genrpg.Shared.Accounts.WebApi.NewVersions;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.DataStores.Categories.PlayerData.Units;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.Purchasing.PlayerData;
using Genrpg.Shared.Trader.Caravans.PlayerData;
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
            CaravanData caravanData = null;

            context.acct = (await _serverRepoService.Search<GameAccount>(x => x.AccountId == request.AccountId && !x.Deleted)).FirstOrDefault();

            if (context.acct == null)
            {
                context.acct = new GameAccount()
                {
                    Id = sessionData.Id, // Not good idea if we want to have different users per account (for deletion)
                    AccountId = request.AccountId,
                    CreationDate = DateTime.UtcNow,
                };
            }

            context.acct.SessionId = HashUtils.NewUUId();
            context.acct.ClientVersion = request.ClientVersion;
            context.acct.ClientPlatformName = request.ClientPlatformName;
            context.Set(context.acct);

            context.user = await context.GetAsync<CoreUserData>();
            context.user.DataOverrides.GameDataCheckTime = request.ClientGameDataSaveTime;
            context.user.ClientVersion = context.acct.ClientVersion;

            List<IUnitData> allUserData = await _loginPlayerDataService.LoadPlayerDataOnLogin(context, null);

            List<IGameSettingsLoader> loaders = _gameDataService.GetAllLoaders();

            _gameDataService.GetClientSettings(context.Responses, context.user, true);

            UpdatePublicUser(sessionData, context.acct);

            _cloudCommsService.SendQueueMessage(CloudServerNames.Player, new LoginUser() { Id = context.acct.Id, Name = "User" + context.acct.Id });

            await _purchasingService.RetryFailedValidation(context, token);

            PlayerStoreOfferData offerData = await _purchasingService.GetCurrentStores(context, context.user, true, token);

            GameAuthResponse response = new GameAuthResponse()
            {
                GameAccount = _serializer.ConvertType<GameAccount, GameAccount>(context.acct),
                CharacterStubs = await _playerDataService.LoadCharacterStubs(context.acct.Id),
                MapStubs = _webServerService.GetMapStubs().Stubs,
                UserData = await _playerDataService.MapToClientDto(context.user, allUserData),
                OfferData = offerData,
            };
            context.Responses.AddFront(response);
        }

        private void UpdatePublicUser(AccountSessionData account, GameAccount user)
        {
            // Just always make new files and save them.

            PublicUser publicUser = new PublicUser() { Id = user.Id };
            publicUser.Name = account.ShareId;
            _serverRepoService.QueueSave(publicUser);

        }

    }
}
