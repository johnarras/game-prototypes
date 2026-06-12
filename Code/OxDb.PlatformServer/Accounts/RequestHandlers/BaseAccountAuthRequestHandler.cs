using MongoDB.Driver;
using OxDb.PlatformServer.Accounts.PlayerData;
using OxDb.PlatformServer.Accounts.Services;
using OxDb.ServerCore.CloudComms.Services;
using OxDb.ServerCore.Crypto.Services;
using OxDb.ServerCore.GameSettings.Services;
using OxDb.SharedCore.DataStores.Interfaces;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedCore.Names.Services;
using OxDb.SharedCore.Utils;
using OxDb.SharedCore.Website.Responses.Core;
using OxDb.SharedPlatform.Accounts.PublicData;
using OxDb.SharedPlatform.Accounts.WebApi.AccountAuth;

namespace OxDb.PlatformServer.Accounts.RequestHandlers
{
    public abstract class BaseAccountAuthRequestHandler<TRequest> : IAccountAuthRequestHandler where TRequest : IAccountAuthRequest
    {

        protected ILogService _logService = null!;
        protected ISearchRepositoryService _repoService = null!;
        protected IServerGameDataService _gameDataService = null!;
        protected ICloudCommsService _cloudCommsService = null!;
        protected IAccountService _accountService = null!;
        protected ICryptoService _cryptoService = null!;
        protected INameValidationService _nameValidaValidationService = null!;

        protected abstract Task HandleRequestInternal(IWebContext context, TRequest request, CancellationToken token);

        public Type HelperKey => typeof(TRequest);

        public async Task Execute(IWebContext context, IAccountAuthRequest request, CancellationToken token)
        {
            await HandleRequestInternal(context, (TRequest)request, token);
        }

        protected async Task AfterAuthSuccess(IWebContext context, Account account, IAccountAuthRequest request)
        {
            ProductRecord prodRecord = account.Products.FirstOrDefault(x => x.ProductId == request.ProductId)!;

            string installSource = request.InstallSource;

            if (string.IsNullOrEmpty(installSource))
            {
                installSource = account.InstallSource;
            }

            bool justAddedProduct = false;
            if (prodRecord == null)
            {
                justAddedProduct = true;
                prodRecord = new ProductRecord()
                {
                    ProductId = request.ProductId,
                    ProductUserId = await _accountService.GetNewUserId(),
                    InstallSource = installSource,
                };
                account.Products.Add(prodRecord);
            }

            if (string.IsNullOrEmpty(prodRecord.InstallSource) && !string.IsNullOrEmpty(installSource))
            {
                prodRecord.InstallSource = installSource;
            }

            if (!string.IsNullOrEmpty(prodRecord.InstallSource) && installSource != prodRecord.InstallSource)
            {
                installSource = prodRecord.InstallSource;
            }

            if (string.IsNullOrEmpty(prodRecord.ProductUserId))
            {
                prodRecord.ProductUserId = await _accountService.GetNewUserId();
            }

            AccountSessionData sessionData = new AccountSessionData()
            {
                Id = account.Id,
                AccountSessionId = HashUtils.NewGuid(),
                DisplayName = account.DisplayName,
            };

            AuthRecord authRecord = account.AuthRecords.FirstOrDefault(x => x.DeviceId == request.DeviceId)!;

            string clientLoginToken;
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

            AccountAuthResponse response = new AccountAuthResponse()
            {
                AccountId = account.Id,
                LoginToken = clientLoginToken,
                AccountSessionId = sessionData.AccountSessionId,
                ProductUserId = prodRecord.ProductUserId,
                DataBits = prodRecord.DataBits,
                ProductId = prodRecord.ProductId,
                DisplayName = account.DisplayName,
                InstallSource = account.InstallSource,
            };

            if (justAddedProduct)
            {
                _accountService.AddAccountToProductGraph(account, request.ProductId, request.ReferrerId, account.Products.Count > 1);
            }
            await UpdatePublicAccount(account);

            context.AddResponse(response);

        }

        protected async Task UpdatePublicAccount(Account account)
        {
            // Just always make new files and save them.

            PublicAccount publicAccount = new PublicAccount() { Id = account.Id };

            publicAccount.DisplayName = account.DisplayName;
            await _repoService.Save(publicAccount);
        }

    }
}


