using Genrpg.RequestServer.AuthRequests.Constants;
using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.PlayerData.Services;
using Genrpg.RequestServer.Services.WebServer;
using Genrpg.ServerShared.Accounts.Services;
using Genrpg.ServerShared.CloudComms.Services;
using Genrpg.ServerShared.Crypto.Services;
using Genrpg.ServerShared.GameSettings.Services;
using Genrpg.ServerShared.PlayerData;
using Genrpg.Shared.Accounts.PlayerData;
using Genrpg.Shared.Accounts.WebApi.Login;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Website.Interfaces;
using Genrpg.Shared.Website.Messages.Error;
using MongoDB.Driver;

namespace Genrpg.RequestServer.AuthRequests.AccountAuthRequestHandlers
{
    public abstract class BaseAccountAuthRequestHandler<TRequest> : IAccountAuthRequestHandler where TRequest : IAccountAuthRequest
    {

        protected IPlayerDataService _playerDataService = null!;
        protected ILoginPlayerDataService _loginPlayerDataService = null!;
        protected ILogService _logService = null!;
        protected ISearchRepositoryService _repoService = null!;
        protected IWebServerService _loginServerService = null!;
        protected IServerGameDataService _gameDataService = null!;
        protected ICloudCommsService _cloudCommsService = null!;
        protected IWebServerService _webServerService = null!;
        protected IAccountService _accountService = null!;
        protected ICryptoService _cryptoService = null!;

        protected abstract Task HandleRequestInternal(WebContext context, TRequest request, CancellationToken token);

        public Type HelperKey => typeof(TRequest);

        public virtual async Task Reset()
        {
            await Task.CompletedTask;
        }

        public async Task Execute(WebContext context, IAccountAuthRequest request, CancellationToken token)
        {
            await HandleRequestInternal(context, (TRequest)request, token);
        }
        protected void ShowError(WebContext context, string msg)
        {
            context.AddResponse(new ErrorResponse() { Error = msg });
        }

        protected async Task AfterAuthSuccess(WebContext context, Account account, IAccountAuthRequest request, EAuthResponse authResponse)
        {
            ProductRecord prodRecord = account.Products.FirstOrDefault(x => x.ProductId == request.ProductId);

            if (prodRecord == null)
            {
                prodRecord = new ProductRecord()
                {
                    ProductId = request.ProductId,
                };
                account.Products.Add(prodRecord);
            }
            AccountSessionData sessionData = new AccountSessionData()
            {
                Id = account.Id,
                SessionToken = HashUtils.NewGuid(),
                ShareId = account.ShareId,
            };

            AuthRecord authRecord = account.AuthRecords.FirstOrDefault(x => x.DeviceId == request.DeviceId);

            string clientLoginToken = null;
            if (authRecord == null)
            {
                authRecord = new AuthRecord()
                {
                    DeviceId = request.DeviceId,
                };
                account.AuthRecords.Add(authRecord);
            }
            clientLoginToken = _cryptoService.GetRandomBytes();
            authRecord.TokenSalt = _cryptoService.GetRandomBytes();
            authRecord.TokenHash = _cryptoService.GetPasswordHash(authRecord.TokenSalt, clientLoginToken);

            List<Task> allTasks = new List<Task>();
            allTasks.Add(_repoService.Save(sessionData));
            allTasks.Add(_repoService.Save(account));

            await Task.WhenAll(allTasks);

            AccountLoginResponse response = new AccountLoginResponse()
            {
                AccountId = account.Id,
                LoginToken = clientLoginToken,
                SessionToken = sessionData.SessionToken,
                GameUserId = account.Id, // This can be different.
            };

            _accountService.AddAccountToProductGraph(account, request.ProductId, request.ReferrerId);

            await UpdatePublicAccount(account);

            context.AddResponse(response);

        }


        private async Task UpdatePublicAccount(Account account)
        {
            // Just always make new files and save them.

            PublicAccount publicAccount = new PublicAccount() { Id = account.Id };

            publicAccount.Name = account.ShareId;
            await _repoService.Save(publicAccount);
        }

        protected EAuthResponse ExistingPasswordIsOk(Account account, IAccountAuthRequest request)
        {
            string newPasswordHash = _cryptoService.GetPasswordHash(account.PasswordSalt, request.Password);

            if (newPasswordHash == account.PasswordHash)
            {
                return EAuthResponse.UsedPassword;
            }

            AuthRecord authRecord = account.AuthRecords.FirstOrDefault(x => x.DeviceId == request.DeviceId);

            if (authRecord == null)
            {
                return EAuthResponse.Failure;
            }

            string newTokenHash = _cryptoService.GetPasswordHash(authRecord.TokenSalt, request.Password);

            if (newTokenHash == authRecord.TokenHash)
            {
                return EAuthResponse.UsedToken;
            }
            else
            {
                return EAuthResponse.Failure;
            }
        }
    }
}


