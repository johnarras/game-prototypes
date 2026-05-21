using OxDb.PlatformServer.Accounts.Constants;
using OxDb.PlatformServer.Accounts.PlayerData;
using OxDb.ServerCore.Crypto.Services;
using OxDb.ServerCore.DataStores.Services;
using OxDb.ServerCore.Platform.Constants;
using OxDb.ServerCore.Platform.WebApi;
using OxDb.SharedCore.DataStores.Indexes;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Tasks.Services;

namespace OxDb.PlatformServer.Accounts.Services
{
    public class AccountService : IAccountService
    {

        private IFullRepositoryService _serverRepositoryService = null;
        private ITaskService _taskService = null;
        private ICryptoService _cryptoService = null;

        public async Task Initialize(CancellationToken token)
        {
            List<Task> tasks = new List<Task>();
            CreateIndexData data = new CreateIndexData(typeof(Account));
            data.Configs.Add(new IndexConfig() { MemberName = nameof(Account.LowerShareId), Unique = true });
            data.Configs.Add(new IndexConfig() { MemberName = nameof(Account.LowerEmail), Unique = true });
            data.Configs.Add(new IndexConfig() { MemberName = nameof(Account.LowerName) });
            data.Configs.Add(new IndexConfig() { MemberName = nameof(Account.ReferrerAccountId) });
            tasks.Add(_serverRepositoryService.CreateIndexes(data));

            data = new CreateIndexData(typeof(AccountConnection));
            data.Configs.Add(new IndexConfig() { MemberName = nameof(AccountConnection.AccountId) });
            data.Configs.Add(new IndexConfig() { MemberName = nameof(AccountConnection.Index) });
            data.Configs.Add(new IndexConfig() { MemberName = nameof(AccountConnection.ProductId) });
            tasks.Add(_serverRepositoryService.CreateIndexes(data));

            data = new CreateIndexData(typeof(ConnectionCount));
            data.Configs.Add(new IndexConfig() { MemberName = nameof(ConnectionCount.AccountId) });
            data.Configs.Add(new IndexConfig() { MemberName = nameof(ConnectionCount.Index) });
            data.Configs.Add(new IndexConfig() { MemberName = nameof(ConnectionCount.ProductId) });
            tasks.Add(_serverRepositoryService.CreateIndexes(data));

            await Task.WhenAll(tasks);

        }


        public void AddAccountToProductGraph(Account account, long accountProductId, string referrerId, bool justAddNewProduct)
        {
            _taskService.ForgetTask(AddAccountToProductGraphAsync(account, accountProductId, referrerId, justAddNewProduct), false);
        }

        private async Task AddAccountToProductGraphAsync(Account account, long accountProductId, string referrerId, bool justAddNewProduct)
        {
            List<long> productIds = new List<long>();

            if (!justAddNewProduct)
            {
                productIds.Add(AccountConstants.CompanyProductId);
            }

            if (accountProductId > AccountConstants.CompanyProductId)
            {
                productIds.Add(accountProductId);
            }

            string referrerAccountId = account.ReferrerAccountId;

            if (!String.IsNullOrEmpty(referrerId))
            {
                Account referrerAccount = (await _serverRepositoryService.Search<Account>(x => x.LowerShareId == referrerId.ToLower())).FirstOrDefault();
                if (referrerAccount != null)
                {
                    referrerAccountId = referrerAccount.Id;
                }
            }

            if (string.IsNullOrEmpty(referrerAccountId))
            {
                foreach (long productId in productIds)
                {
                    for (int index = AccountConstants.MinConnectionIndex; index <= AccountConstants.MaxConnectionIndex; index++)
                    {
                        await AddConnections(account.Id, null, productId, index);
                    }
                }
                return;
            }

            foreach (long productId in productIds)
            {
                for (int index = AccountConstants.MinConnectionIndex; index <= AccountConstants.MaxConnectionIndex; index++)
                {
                    List<AccountConnection> myConnections = await _serverRepositoryService.Search<AccountConnection>(x =>
                    x.AccountId == account.Id &&
                    x.ProductId == accountProductId &&
                    x.Index == index);

                    if (myConnections.Count > 0)
                    {
                        continue;
                    }

                    List<AccountConnection> referrerConnections = await _serverRepositoryService.Search<AccountConnection>(x =>
                           x.AccountId == referrerAccountId &&
                           x.ProductId == productId &&
                           x.Index == index);

                    referrerConnections = referrerConnections.OrderBy(x => x.Depth).ToList();

                    string finalReferrerId = await GetFinalReferrerId(referrerAccountId, referrerConnections, productId, index);

                    await AddConnections(account.Id, finalReferrerId, productId, index);

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
                List<AccountConnection> childConnections = await _serverRepositoryService.Search<AccountConnection>(x =>
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

            ConnectionCount myCount = (await _serverRepositoryService.Search<ConnectionCount>(c => c.AccountId == accountId &&
            c.ProductId == productId && c.Index == index)).FirstOrDefault();

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

                await _serverRepositoryService.Save(myCount);
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

            await _serverRepositoryService.Save(myConn);

            if (string.IsNullOrEmpty(referrerAccountId))
            {
                return;
            }

            List<Task> connectionTasks = new List<Task>();

            // Update based on parent connections.
            List<AccountConnection> referrerConnections = await _serverRepositoryService.Search<AccountConnection>(x =>
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
                connectionTasks.Add(_serverRepositoryService.Save(newConn));
            }

            await Task.WhenAll(connectionTasks);

            // Now increment the values for the connections above.

            List<string> referrerAccountIds = referrerConnections.Select(x => x.ReferrerId).ToList();

            referrerAccountIds.Add(referrerAccountId);

            List<ConnectionCount> connectionCounts = await _serverRepositoryService.Search<ConnectionCount>(x =>
            referrerAccountIds.Contains(x.AccountId) &&
            x.ProductId == productId &&
            x.Index == index);

            List<Task> incTasks = new List<Task>();

            List<string> docIds = connectionCounts.Select(x => x.Id).ToList();

            ConnectionCount mainCount = connectionCounts.FirstOrDefault(x => x.AccountId == referrerAccountId);

            if (mainCount != null)
            {
                incTasks.Add(_serverRepositoryService.AtomicIncrement<ConnectionCount>(mainCount.Id, nameof(ConnectionCount.DirectCount), 1));
            }

            foreach (ConnectionCount connectionCount in connectionCounts)
            {
                incTasks.Add(_serverRepositoryService.AtomicIncrement<ConnectionCount>(connectionCount.Id, nameof(ConnectionCount.ViralCount), 1));
            }

            await Task.WhenAll(incTasks);

        }

        // This needs to be improved obviously.
        private readonly string[] _nameBlacklist = {
            "fuck", "shit", "nazi", "cunt",
            "piss", "slut", "nigg", "damn",
            "hell", "asshole", "fuk", "shyt",
            "coc", "dik", "vag",

        };
        private bool IsInappropriate(string base58Id)
        {
            string lowerId = base58Id.ToLower();

            // Check for direct matches or leetspeak subs
            // You can expand this to check for '5' as 's', etc.
            string normalized = lowerId
                .Replace('5', 's')
                .Replace('1', 'i')
                .Replace('4', 'a')
                .Replace('8', 'b')
                .Replace('0', 'o')
                .Replace('3', 'e')
                .Replace('6', 'g')
                ;

            return _nameBlacklist.Any(word => normalized.Contains(word));
        }

        public async Task<string> GetNewUserId()
        {
            string encoded;

            int times = 0;
            do
            {
                encoded = HashUtils.GetIdFromVal(BitConverter.ToInt64(_cryptoService.GetRandomBytes(8), 0));

                if (!IsInappropriate(encoded))
                {
                    ClaimedAccountId claimedId = await _serverRepositoryService.Load<ClaimedAccountId>(encoded);

                    if (claimedId == null)
                    {
                        claimedId = new ClaimedAccountId() { Id = encoded };
                        await _serverRepositoryService.Save(claimedId);
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

            Account account = await _serverRepositoryService.Load<Account>(request.AccountId);

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

            AccountSessionData sessionData = await _serverRepositoryService.Load<AccountSessionData>(account.Id);

            if (sessionData == null || sessionData.SessionId != request.SessionId)
            {
                response.State = EPlatformAuthStates.IncorrectSessionId;
                return response;
            }

            // 
            if ((record.DataBits & ~request.DataBits) == 0)
            {
                record.DataBits = request.DataBits;
                await _serverRepositoryService.Save(account);
            }
            else
            {
                response.State = EPlatformAuthStates.ExistingGameDataIsMissing;
                return response;
            }

            response.State = EPlatformAuthStates.Success;
            return response;
        }
    }
}


