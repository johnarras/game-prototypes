using MongoDB.Driver;
using OxDb.PlatformServer.AccountAuthRequests.Constants;
using OxDb.PlatformServer.Accounts.PlayerData;
using OxDb.PlatformServer.Accounts.Services;
using OxDb.ServerCore.CloudComms.Services;
using OxDb.ServerCore.Crypto.Services;
using OxDb.ServerCore.GameSettings.Services;
using OxDb.SharedCore.DataStores.Interfaces;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedCore.Website.Interfaces;
using OxDb.SharedCore.Website.Responses.Core;
using OxDb.SharedCore.Website.Responses.Errors;
using OxDb.SharedPlatform.Accounts.PublicData;
using OxDb.SharedPlatform.Accounts.WebApi.Login;

namespace OxDb.PlatformServer.AccountAuthRequests.RequestHandlers
{
    public abstract class BaseAccountAuthRequestHandler<TRequest> : IAccountAuthRequestHandler where TRequest : IAccountAuthRequest
    {

        protected ILogService _logService = null!;
        protected ISearchRepositoryService _repoService = null!;
        protected IServerGameDataService _gameDataService = null!;
        protected ICloudCommsService _cloudCommsService = null!;
        protected IAccountService _accountService = null!;
        protected ICryptoService _cryptoService = null!;

        protected abstract Task HandleRequestInternal(IWebContext context, TRequest request, CancellationToken token);

        public Type HelperKey => typeof(TRequest);

        public virtual async Task Reset()
        {
            await Task.CompletedTask;
        }

        public async Task Execute(IWebContext context, IAccountAuthRequest request, CancellationToken token)
        {
            await HandleRequestInternal(context, (TRequest)request, token);
        }
        protected void ShowError(IWebContext context, string msg)
        {
            context.AddResponse(new ErrorResponse() { Error = msg });
        }

        protected async Task AfterAuthSuccess(IWebContext context, Account account, IAccountAuthRequest request, EAuthResponse authResponse)
        {
            ProductRecord prodRecord = account.Products.FirstOrDefault(x => x.ProductId == request.ProductId);

            bool justAddedProduct = false;
            if (prodRecord == null)
            {
                justAddedProduct = true;
                prodRecord = new ProductRecord()
                {
                    ProductId = request.ProductId,
                    ProductUserId = await _accountService.GetNewUserId(),
                };
                account.Products.Add(prodRecord);
            }


            if (string.IsNullOrEmpty(prodRecord.ProductUserId))
            {
                prodRecord.ProductUserId = await _accountService.GetNewUserId();
            }

            AccountSessionData sessionData = new AccountSessionData()
            {
                Id = account.Id,
                SessionId = HashUtils.NewGuid(),
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
            clientLoginToken = _cryptoService.GetRandomByteString(16);
            authRecord.TokenSalt = _cryptoService.GetRandomByteString(16);
            authRecord.TokenHash = _cryptoService.GetPasswordHash(authRecord.TokenSalt, clientLoginToken);

            List<Task> allTasks = new List<Task>();
            allTasks.Add(_repoService.Save(sessionData));
            allTasks.Add(_repoService.Save(account));

            await Task.WhenAll(allTasks);

            AccountLoginResponse response = new AccountLoginResponse()
            {
                AccountId = account.Id,
                LoginToken = clientLoginToken,
                SessionId = sessionData.SessionId,
                ProductUserId = prodRecord.ProductUserId,
                DataBits = prodRecord.DataBits,
                ProductId = prodRecord.ProductId,
                ShareId = account.ShareId,
            };

            if (justAddedProduct)
            {
                _accountService.AddAccountToProductGraph(account, request.ProductId, request.ReferrerId, account.Products.Count > 1);
            }
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


