using MongoDB.Bson;
using OxDb.PlatformServer.Accounts.Constants;
using OxDb.PlatformServer.Accounts.PlayerData;
using OxDb.ServerCore.Crypto.Services;
using OxDb.ServerCore.DataStores.Services;
using OxDb.ServerCore.Platform.Constants;
using OxDb.ServerCore.Platform.WebApi;
using OxDb.SharedCore.DataStores.Indexes;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Names.Services;
using OxDb.SharedCore.Tasks.Services;
using OxDb.SharedCore.Utils;
using OxDb.SharedPlatform.Accounts.WebApi.AccountAuth;

namespace OxDb.PlatformServer.Accounts.Services
{
    public interface IAccountService : IInitializable
    {
        void AddAccountToProductGraph(AddAccountConnectionArgs args);

        Task<string> GetNewUserId();

        Task<ProductToPlatformAuthResponse> HandleGameAuthRequest(ProductToPlatformAuthRequest request);

        Task<Account> CreateNewAccount(IAccountAuthRequest request);
    }

    public class AccountService : IAccountService
    {

        private IFullRepositoryService _repoService = null;
        private ITaskService _taskService = null;
        private ICryptoService _cryptoService = null;
        private INameValidationService _nameValidationService = null;

        public async Task Initialize(CancellationToken token)
        {
            List<Task> tasks = new List<Task>();
            CreateIndexData data = new CreateIndexData(typeof(Account));
            data.Configs.Add(new IndexConfig() { MemberName = nameof(Account.LowerDisplayName) });
            data.Configs.Add(new IndexConfig() { MemberName = nameof(Account.LowerEmail) });

            data.Configs.Add(new IndexConfig() { MemberName = nameof(Account.DisplayName) });
            data.Configs.Add(new IndexConfig() { MemberName = nameof(Account.ReferrerAccountId) });


            data.Configs.Add(new IndexConfig() { MemberName = nameof(Account.GooglePlayUserId) });
            data.Configs.Add(new IndexConfig() { MemberName = nameof(Account.LowerGoogleEmail) });


            data.Configs.Add(new IndexConfig() { MemberName = nameof(Account.AppleUserId) });
            data.Configs.Add(new IndexConfig() { MemberName = nameof(Account.LowerAppleEmail) });


            data.Configs.Add(new IndexConfig() { MemberName = nameof(Account.FacebookUserId) });
            data.Configs.Add(new IndexConfig() { MemberName = nameof(Account.LowerFacebookEmail) });

            await _repoService.CreateIndexes(data);

            data = new CreateIndexData(typeof(AccountConnection));
            data.Configs.Add(new IndexConfig() { MemberName = nameof(AccountConnection.AccountId) });
            data.Configs.Add(new IndexConfig() { MemberName = nameof(AccountConnection.Index) });
            data.Configs.Add(new IndexConfig() { MemberName = nameof(AccountConnection.ProductId) });
            await _repoService.CreateIndexes(data);

            data = new CreateIndexData(typeof(ConnectionCount));
            data.Configs.Add(new IndexConfig() { MemberName = nameof(ConnectionCount.AccountId) });
            data.Configs.Add(new IndexConfig() { MemberName = nameof(ConnectionCount.Index) });
            data.Configs.Add(new IndexConfig() { MemberName = nameof(ConnectionCount.ProductId) });
            await _repoService.CreateIndexes(data);

            await Task.WhenAll(tasks);

        }


        public void AddAccountToProductGraph(AddAccountConnectionArgs args)
        {
            _taskService.ForgetTask(AddAccountToProductGraphAsync(args), false);
        }

        private async Task AddAccountToProductGraphAsync(AddAccountConnectionArgs args)
        {
            List<long> productIds = new List<long>();

            if (!args.JustAddedNewProduct)
            {
                productIds.Add(AccountConstants.CompanyProductId);
            }

            if (args.AccountProductId > AccountConstants.CompanyProductId)
            {
                productIds.Add(args.AccountProductId);
            }

            string referrerDisplayName = args.ReferrerDisplayName;

            if (!string.IsNullOrEmpty(referrerDisplayName))
            {
                Account referrerAccount = (await _repoService.Search<Account>(x => x.LowerDisplayName == referrerDisplayName.ToLower())).FirstOrDefault()!;
                if (referrerAccount != null)
                {
                    referrerDisplayName = referrerAccount.Id;
                }
            }

            if (string.IsNullOrEmpty(referrerDisplayName))
            {
                foreach (long productId in productIds)
                {
                    for (int index = AccountConstants.MinConnectionIndex; index <= AccountConstants.MaxConnectionIndex; index++)
                    {
                        await AddConnections(args.AccountId, null!, productId, index);
                    }
                }
                return;
            }

            foreach (long productId in productIds)
            {
                for (int index = AccountConstants.MinConnectionIndex; index <= AccountConstants.MaxConnectionIndex; index++)
                {
                    List<AccountConnection> myConnections = await _repoService.Search<AccountConnection>(x =>
                    x.AccountId == args.AccountId &&
                    x.ProductId == args.AccountProductId &&
                    x.Index == index);

                    if (myConnections.Count > 0)
                    {
                        continue;
                    }

                    List<AccountConnection> referrerConnections = await _repoService.Search<AccountConnection>(x =>
                           x.AccountId == referrerDisplayName &&
                           x.ProductId == productId &&
                           x.Index == index);

                    referrerConnections = referrerConnections.OrderBy(x => x.Depth).ToList();

                    string finalReferrerId = await GetFinalReferrerId(referrerDisplayName, referrerConnections, productId, index);

                    await AddConnections(args.AccountId, finalReferrerId, productId, index);

                }
            }

            await Task.CompletedTask;
        }

        private async Task<string> GetFinalReferrerId(string startReferrerId, List<AccountConnection> orderedReferrerConnections, long productId, int index)
        {
            if (index == AccountConstants.MinConnectionIndex || orderedReferrerConnections.Count < 1)
            {
                return startReferrerId;
            }

            AccountConnection topAccount = orderedReferrerConnections.Last();

            int checkTimes = 0;
            while (++checkTimes < 20)
            {
                List<AccountConnection> childConnections = await _repoService.Search<AccountConnection>(x =>
                           x.ReferrerId == topAccount.Id &&
                           x.ProductId == productId &&
                           x.Index == index);

                if (childConnections.Count < AccountConstants.MaxConnectionFanout - 1 ||
                    (childConnections.Count == AccountConstants.MaxConnectionFanout - 1 &&
                    Random.Shared.NextDouble() < 0.1f))
                {
                    return topAccount.AccountId;
                }

                topAccount = childConnections[Random.Shared.Next(0, childConnections.Count)];

            }
            return startReferrerId;
        }

        private async Task AddConnections(string accountId, string referrerAccountId, long productId, int index)
        {
            // Add my counts.

            ConnectionCount myCount = (await _repoService.Search<ConnectionCount>(c => c.AccountId == accountId &&
            c.ProductId == productId && c.Index == index)).FirstOrDefault()!;

            if (myCount == null)
            {
                myCount = new ConnectionCount()
                {
                    Id = HashUtils.NewGuid(),
                    AccountId = accountId,
                    DirectCount = 0,
                    ViralCount = 0,
                    ProductId = productId,
                    Index = index,
                };

                await _repoService.Save(myCount);
            }
            // Save my connection.
            AccountConnection myConn = new AccountConnection()
            {
                Id = HashUtils.NewGuid(),
                AccountId = accountId,
                ReferrerId = referrerAccountId,
                Depth = 1,
                ProductId = productId,
                Index = index
            };

            await _repoService.Save(myConn);

            if (string.IsNullOrEmpty(referrerAccountId))
            {
                return;
            }

            List<Task> connectionTasks = new List<Task>();

            // Update based on parent connections.
            List<AccountConnection> referrerConnections = await _repoService.Search<AccountConnection>(x =>
                   x.AccountId == referrerAccountId &&
                   x.ProductId == productId &&
                   x.Index == index);

            foreach (AccountConnection connection in referrerConnections)
            {
                AccountConnection newConn = new AccountConnection()
                {
                    Id = HashUtils.NewGuid(),
                    AccountId = accountId,
                    ReferrerId = connection.ReferrerId,
                    ProductId = productId,
                    Index = index,
                    Depth = connection.Depth + 1,
                };
                connectionTasks.Add(_repoService.Save(newConn));
            }

            await Task.WhenAll(connectionTasks);

            // Now increment the values for the connections above.

            List<string> referrerAccountIds = referrerConnections.Select(x => x.ReferrerId).ToList();

            referrerAccountIds.Add(referrerAccountId);

            List<ConnectionCount> connectionCounts = await _repoService.Search<ConnectionCount>(x =>
            referrerAccountIds.Contains(x.AccountId) &&
            x.ProductId == productId &&
            x.Index == index);

            List<Task> incTasks = new List<Task>();

            List<string> docIds = connectionCounts.Select(x => x.Id).ToList();

            ConnectionCount mainCount = connectionCounts.FirstOrDefault(x => x.AccountId == referrerAccountId);

            if (mainCount != null)
            {
                incTasks.Add(_repoService.AtomicIncrement<ConnectionCount>(mainCount.Id, nameof(ConnectionCount.DirectCount), 1));
            }

            foreach (ConnectionCount connectionCount in connectionCounts)
            {
                incTasks.Add(_repoService.AtomicIncrement<ConnectionCount>(connectionCount.Id, nameof(ConnectionCount.ViralCount), 1));
            }

            await Task.WhenAll(incTasks);

        }


        public async Task<string> GetNewUserId()
        {
            string encoded;

            int times = 0;
            do
            {
                encoded = HashUtils.GetIdFromVal(BitConverter.ToInt64(_cryptoService.GetRandomBytes(8), 0));


                if (!await _nameValidationService.ContainsSwearWord(encoded))
                {
                    ClaimedAccountId claimedId = await _repoService.Load<ClaimedAccountId>(encoded);

                    if (claimedId == null)
                    {
                        claimedId = new ClaimedAccountId() { Id = encoded };
                        await _repoService.Save(claimedId);
                        return encoded;
                    }
                }
            }
            while (++times < 10);

            encoded = HashUtils.NewGuid();

            return encoded;
        }

        public async Task<ProductToPlatformAuthResponse> HandleGameAuthRequest(ProductToPlatformAuthRequest request)
        {
            ProductToPlatformAuthResponse response = new ProductToPlatformAuthResponse();

            Account account = await _repoService.Load<Account>(request.AccountId);

            if (account == null)
            {
                response.State = EPlatformAuthStates.AccountDoesNotExist;
                return response;
            }

            ProductRecord record = account.Products.FirstOrDefault(x => x.ProductId == request.ProductId);

            if (record == null)
            {
                response.State = EPlatformAuthStates.ProductWasNotAdded;
                return response;
            }

            if (request.DataBits == 0 && record.DataBits != 0)
            {
                response.State = EPlatformAuthStates.ProductAccountWasAlreadyCreated;
                return response;
            }

            if (record.ProductUserId != request.ProductUserId)
            {
                response.State = EPlatformAuthStates.ProductUserIdDoesNotMatch;
                return response;
            }

            AccountSessionData sessionData = await _repoService.Load<AccountSessionData>(account.Id);

            if (sessionData == null || sessionData.AccountSessionId != request.AccountSessionId)
            {
                response.State = EPlatformAuthStates.IncorrectSessionId;
                return response;
            }

            // 
            if ((record.DataBits & ~request.DataBits) == 0)
            {
                record.DataBits = request.DataBits;
                await _repoService.Save(account);
            }
            else
            {
                response.State = EPlatformAuthStates.ExistingGameDataIsMissing;
                return response;
            }

            response.State = EPlatformAuthStates.Success;
            return response;
        }



        public async Task<Account> CreateNewAccount(IAccountAuthRequest request)
        {

            Account referrerAcount = null;
            if (!string.IsNullOrEmpty(request.ReferrerId))
            {
                referrerAcount = await _repoService.Load<Account>(request.ReferrerId);
            }

            string newId = await GetNewUserId();
            Account acc = new Account()
            {
                Id = newId,
                CreatedOn = DateTime.UtcNow,
                OriginalProductId = request.ProductId,
                InstallSource = request.InstallSource,
                Flags = 0,
                ReferrerAccountId = referrerAcount?.Id ?? "",
            };

            acc.DisplayName = newId + (Random.Shared.Next() % 10000);

            return acc;
        }
    }
}


