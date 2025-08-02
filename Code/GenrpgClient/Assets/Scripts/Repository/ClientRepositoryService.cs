
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Indexes;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;
using System.Threading.Tasks;
using Genrpg.Shared.Entities.Utils;
using Genrpg.Shared.Logging.Interfaces;
using System.Threading;
using Genrpg.Shared.Setup.Constants;
using Genrpg.Shared.Utils;

namespace Assets.Scripts.Repository
{
    public class ClientRepositoryService : IRepositoryService
    {
        

        private ILogService _logService = null;
        private IClientAppService _clientAppService;
        private ITextSerializer _serializer;

        public async Task Initialize(CancellationToken token)
        {
            await Task.CompletedTask;
        }
        public ClientRepositoryService()
        {
        }

        public int SetupPriorityAscending() { return SetupPriorities.Repositories; }

        public async Task PrioritySetup(CancellationToken token)
        {
            await Task.CompletedTask;
        }

        public async Task<bool> Delete<T>(T t) where T : class, IStringId
        {
            ClientRepositoryCollection<T> repo = GetRepository<T>();
            return await repo.Delete(t);
        }

        public async Task<object> LoadWithType(Type t, string id)
        {
            IClientRepositoryCollection repo = GetRepositoryFromType(t);
            return await repo.LoadWithType(t, id);
        }

        public async Task<T> Load<T>(string id) where T : class, IStringId
        {
            ClientRepositoryCollection<T> repo = GetRepository<T>();
            return await repo.Load(id);
        }

        public void QueueDelete<T>(T t) where T : class, IStringId
        {
            Delete(t).Wait();
        }

        public void QueueSave<T>(T t) where T : class, IStringId
        {
            Save(t).Wait();
        }

        public async Task<bool> Save<T>(T t, bool verbose = false) where T : class, IStringId
        {
            try
            {
                IClientRepositoryCollection repo = GetRepositoryFromType(t.GetType());
                return await repo.Save(t, verbose);
            }
            catch (Exception e)
            {
                Debug.Log("EXC: " + e.Message + " " + e.StackTrace);
            }
            return false;
        }

        public async Task<bool> SaveAll<T>(List<T> list) where T : class, IStringId
        {
            foreach (T t in list)
            {
                await Save(t);
            }
            return true;
        }

        public async Task<bool> StringSave<T>(string id, string data) where T : class, IStringId
        {
            ClientRepositoryCollection<T> repo = GetRepository<T>();
            return await repo.StringSave(id, data);
        }

        public async Task<T> LoadObjectFromString<T>(string id) where T : class, IStringId
        {
            ClientRepositoryCollection<T> repo = GetRepository<T>();
            return await repo.LoadObjectFromString(id);
        }

        public async Task<List<T>> LoadAll<T>(List<string> ids) where T : class, IStringId
        {
            ClientRepositoryCollection<T> repo = GetRepository<T>();
            return await repo.LoadAll(ids);
        }

        public async Task<List<T>> Search<T>(Expression<Func<T, bool>> func, int quantity = 1000, int skip = 0) where T : class, IStringId
        {
            ClientRepositoryCollection<T> repo = GetRepository<T>();
            return await repo.Search(func);
        }

        public Task CreateIndex<T>(CreateIndexData data) where T : class, IStringId
        {
            return Task.CompletedTask;
        }

        public async Task<bool> TransactionSave<T>(List<T> list) where T : class, IStringId
        {
            return await SaveAll(list);
        }

        public void QueueTransactionSave<T>(List<T> list, string queueId) where T : class, IStringId
        {
            SaveAll(list).Wait();
        }

        private Dictionary<Type, object> _repoCache = new Dictionary<Type, object>();
        public IClientRepositoryCollection GetRepositoryFromType (Type t)
        {
            if (_repoCache.TryGetValue(t, out object repo))
            {
                return (IClientRepositoryCollection) repo;
            }

            Type baseRepoType = typeof(ClientRepositoryCollection<>);
            Type genericType = baseRepoType.MakeGenericType(t);
            object newRepo = Activator.CreateInstance(genericType, new object[] { _logService, _clientAppService, _serializer });

            _repoCache[t] = newRepo;

            return (IClientRepositoryCollection) newRepo;
        }

        public ClientRepositoryCollection<T> GetRepository<T>() where T : class, IStringId
        {
            return (ClientRepositoryCollection<T>)GetRepositoryFromType(typeof(T));
        }

        public async Task<bool> DeleteAll<T>(Expression<Func<T, bool>> func) where T : class, IStringId
        {
            await Task.CompletedTask;
            return false;
        }

        // Don't allow this on the client
        public async Task<bool> UpdateDict<T>(string docId, Dictionary<string,object> fieldNameUpdates) where T : class, IStringId
        {
            T doc = await Load<T>(docId);

            if (doc != null)
            {
                foreach (string key in fieldNameUpdates.Keys)
                {
                    EntityUtils.SetObjectValue(doc, key, fieldNameUpdates[key]);
                }

                return await Save(doc);
            }

            await Task.CompletedTask;
            return false;
        }
        public void QueueUpdateDict<T>(string docId, Dictionary<string, object> fieldNameUpdates) where T : class, IStringId
        {
            UpdateDict<T>(docId, fieldNameUpdates).Wait();
        }

        public async Task<bool> UpdateAction<T>(string docId, Action<T> action) where T : class, IStringId
        {
            T doc = await Load<T>(docId);

            if (doc != null)
            {
                action(doc);
                return await Save(doc);
            }

            await Task.CompletedTask;
            return false;
        }

        public void QueueUpdateAction<T>(string docId, Action<T> action) where T : class, IStringId
        {
            UpdateAction<T>(docId, action).Wait();
        }
    }
}
