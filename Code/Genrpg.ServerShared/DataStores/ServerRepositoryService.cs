using Genrpg.ServerShared.Config;
using Genrpg.ServerShared.DataStores.DbQueues;
using Genrpg.ServerShared.DataStores.DbQueues.Actions;
using Genrpg.ServerShared.DataStores.Entities;
using Genrpg.ServerShared.OnlineResources.Interfaces;
using Genrpg.ServerShared.Secrets.Services;
using Genrpg.Shared.Analytics.Services;
using Genrpg.Shared.DataStores.DataGroups;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Indexes;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Setup.Constants;
using Genrpg.Shared.Tasks.Services;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.DataStores
{
    public interface IServerRepositoryService : IRepositoryService
    {
        Task<T> AtomicIncrement<T>(string docId, string fieldName, long increment) where T : class, IStringId;
        Task<T> AtomicAddBits<T>(string docId, string fieldName, long addBits) where T : class, IStringId;
        Task<T> AtomicRemoveBits<T>(string docId, string fieldName, long removeBits) where T : class, IStringId;
    }

    public class ServerRepositoryService : IServerRepositoryService
    {

        private ISecretsProvider _secretsProvider = null!;
        private ITextSerializer _textSerializer = null!;
        private IOnlineResourceProvider _resourceProvider = null!;
        private IAnalyticsService _analyticsService = null!;

        public async Task Initialize(CancellationToken toke)
        {
            await Task.CompletedTask;
        }

        const int QueueCount = 4;

        private List<DbQueue> _queues = null;
        private ILogService _logService = null;
        private IServerConfig _config = null;
        private ITaskService _taskService = null;

        private Dictionary<string, string> _environments = new Dictionary<string, string>();

        private ConcurrentDictionary<string, IRepository> _repos = new ConcurrentDictionary<string, IRepository>();
        private ConcurrentDictionary<Type, IRepository> _repoTypeDict = new ConcurrentDictionary<Type, IRepository>();

        public int SetupPriorityAscending() { return SetupPriorities.Repositories; }

        public async Task PrioritySetup(CancellationToken token)
        {

            _environments = _config.DataEnvs;
            _queues = new List<DbQueue>();
            for (int i = 0; i < QueueCount; i++)
            {
                _queues.Add(new DbQueue(_logService, _taskService, token));
            }


            foreach (EDataCategories category in Enum.GetValues(typeof(EDataCategories)))
            {
                string env = _environments[category.ToString()];
                foreach (ERepoTypes repoType in Enum.GetValues(typeof(ERepoTypes)))
                {
                    InitRepoArgs args = new InitRepoArgs()
                    {
                        Category = category,
                        RepoType = repoType,
                        Env = env,
                    };

                    string typeKey = GetEnvCategoryStoreTypeKey(env, category, repoType);
                    _repos[typeKey] = await _resourceProvider.CreateRepo(args);
                }
            }

            await Task.CompletedTask;
        }

        private string GetEnvCategoryStoreTypeKey(string env, EDataCategories category, ERepoTypes repoType)
        {
            return (env + category.ToString() + repoType.ToString()).ToLower();
        }

        /// <summary>
        /// Find a repository based on the type passed in.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public IRepository FindRepo(Type t)
        {
            if (_repoTypeDict.TryGetValue(t, out IRepository repo))
            {
                return repo;
            }

            DataGroup dataGroup = Attribute.GetCustomAttribute(t, typeof(DataGroup), true) as DataGroup;

            if (dataGroup == null)
            {
                throw new Exception("Missing DataCategory on type " + t.Name);
            }

            string dbEnv = _environments[dataGroup.Category.ToString()];

            string typeKey = GetEnvCategoryStoreTypeKey(dbEnv, dataGroup.Category, dataGroup.RepoType);

            if (_repos.TryGetValue(typeKey, out IRepository existingRepo))
            {
                _repoTypeDict[t] = existingRepo;
                return existingRepo;
            }

            return null;
        }

        public async Task<bool> Delete<T>(T obj) where T : class, IStringId
        {
            IRepository repo = FindRepo(obj.GetType());
            return await repo.Delete(obj);
        }

        public async Task<bool> DeleteAll<T>(Expression<Func<T, bool>> func) where T : class, IStringId
        {
            IRepository repo = FindRepo(typeof(T));
            return await repo.DeleteAll(func);
        }

        public async Task<bool> Save<T>(T obj, bool verbose = false) where T : class, IStringId
        {
            IRepository repo = FindRepo(obj.GetType());
            return await repo.Save(obj, verbose);
        }

        public async Task<T> Load<T>(string id) where T : class, IStringId
        {
            IRepository repo = FindRepo(typeof(T));
            return await repo.Load<T>(id);
        }

        public async Task<bool> SaveAll<T>(List<T> list) where T : class, IStringId
        {
            if (list.Count < 1)
            {
                return true;
            }

            IRepository repo = FindRepo(list[0].GetType());
            return await repo.SaveAll(list);
        }

        public async Task<bool> TransactionSave<T>(List<T> list) where T : class, IStringId
        {
            IRepository repo = FindRepo(list[0].GetType());
            return await repo.TransactionSave(list);
        }

        public void QueueSave<T>(T t) where T : class, IStringId
        {
            SaveAction<T> saveAction = new SaveAction<T>(t, this);
            _queues[StrUtils.GetPrefixIdHash(t.Id) % QueueCount].Enqueue(saveAction);
        }

        public void QueueTransactionSave<T>(List<T> list, string queueId) where T : class, IStringId
        {
            if (list.Count < 1)
            {
                return;
            }

            SaveAction<T> saveAction = new SaveAction<T>(list, this);
            _queues[StrUtils.GetPrefixIdHash(queueId) % QueueCount].Enqueue(saveAction);
        }

        public void QueueDelete<T>(T t) where T : class, IStringId
        {
            DeleteAction<T> deleteAction = new DeleteAction<T>(t, this);
            _queues[StrUtils.GetPrefixIdHash(t.Id) % QueueCount].Enqueue(deleteAction);
        }

        public async Task<List<T>> Search<T>(Expression<Func<T, bool>> func, int quantity = 1000, int skip = 0) where T : class, IStringId
        {
            IRepository repo = FindRepo(typeof(T));
            return await repo.Search<T>(func, quantity, skip);
        }

        public async Task CreateIndex<T>(CreateIndexData data) where T : class, IStringId
        {
            IRepository repo = FindRepo(typeof(T));
            await repo.CreateIndex<T>(data);
            return;
        }

        public async Task<bool> UpdateDict<T>(string docId, Dictionary<string, object> fieldNameUpdates) where T : class, IStringId
        {
            IRepository repo = FindRepo(typeof(T));

            return await repo.UpdateDict<T>(docId, fieldNameUpdates);
        }

        public void QueueUpdateDict<T>(string docId, Dictionary<string, object> fieldNameUpdates) where T : class, IStringId
        {
            UpdateAction<T> updateAction = new UpdateAction<T>(docId, fieldNameUpdates, this);
            _queues[StrUtils.GetPrefixIdHash(docId) % QueueCount].Enqueue(updateAction);
        }


        public async Task<bool> UpdateAction<T>(string docId, Action<T> action) where T : class, IStringId
        {
            IRepository repo = FindRepo(typeof(T));

            return await repo.UpdateAction<T>(docId, action);
        }

        public void QueueUpdateAction<T>(string docId, Action<T> action) where T : class, IStringId
        {
            UpdateAction<T> updateAction = new UpdateAction<T>(docId, action, this);
            _queues[StrUtils.GetPrefixIdHash(docId) % QueueCount].Enqueue(updateAction);
        }

        public async Task<T> AtomicIncrement<T>(string docId, string fieldName, long increment) where T : class, IStringId
        {
            IServerRepository repo = FindRepo(typeof(T)) as IServerRepository;

            return await repo.AtomicIncrement<T>(docId, fieldName, increment);

        }


        public async Task<T> AtomicAddBits<T>(string docId, string fieldName, long addBits) where T : class, IStringId
        {
            IServerRepository repo = FindRepo(typeof(T)) as IServerRepository;

            return await repo.AtomicAddBits<T>(docId, fieldName, addBits);

        }

        public async Task<T> AtomicRemoveBits<T>(string docId, string fieldName, long removeBits) where T : class, IStringId
        {
            IServerRepository repo = FindRepo(typeof(T)) as IServerRepository;

            return await repo.AtomicRemoveBits<T>(docId, fieldName, removeBits);

        }

    }
}
