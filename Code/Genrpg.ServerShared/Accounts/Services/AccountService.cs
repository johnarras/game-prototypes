using Genrpg.ServerShared.DataStores;
using Genrpg.Shared.Accounts.Constants;
using Genrpg.Shared.Accounts.PlayerData;
using Genrpg.Shared.DataStores.Indexes;
using Genrpg.Shared.Tasks.Services;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.Accounts.Services
{
    public class AccountService : IAccountService
    {

        private IFullRepositoryService _serverRepositoryService = null;
        private ITaskService _taskService = null;

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

            AccountIdIncrement increment = await _serverRepositoryService.Load<AccountIdIncrement>(AccountIdIncrement.DocId);

            if (increment == null)
            {
                increment = new AccountIdIncrement() { Id = AccountIdIncrement.DocId };
                await _serverRepositoryService.Save<AccountIdIncrement>(increment);
            }
        }


        public void AddAccountToProductGraph(Account account, long accountProductId, string referrerId)
        {
            _taskService.ForgetTask(AddAccountToProductGraphAsync(account, accountProductId, referrerId), false);
        }

        private async Task AddAccountToProductGraphAsync(Account account, long accountProductId, string referrerId)
        {
            List<long> productIds = new List<long>() { AccountConstants.CompanyProductId };

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

            ConnectionCount myCount = new ConnectionCount()
            {
                Id = HashUtils.NewGuid(),
                AccountId = accountId,
                DirectCount = 0,
                ViralCount = 0,
                ProductId = productId,
                Index = index,
            };

            await _serverRepositoryService.Save(myCount);

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

        public async Task<string> GetNextAccountId()
        {
            AccountIdIncrement increment = await _serverRepositoryService.AtomicIncrement<AccountIdIncrement>(AccountIdIncrement.DocId, nameof(AccountIdIncrement.AccountId), 1) as AccountIdIncrement;

            MyRandom random = new MyRandom(increment.AccountId);

            long newAccountId = random.Next() + AccountConstants.IdStartVal;
            // Feel free to use a GUID or something...I just think it's neat I can autoincrement in mongo. :)
            string newId = HashUtils.GetIdFromVal(newAccountId);
            return newId;
        }
    }
}


