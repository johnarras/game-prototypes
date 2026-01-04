
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Serialization.Interfaces;
using Genrpg.Shared.Setup.Constants;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Repository
{

    public interface IClientRepositoryService : IRepositoryService
    {
        Task<bool> StringSave<T>(string id, string data) where T : class, IStringId;
        Task<T> LoadObjectFromString<T>(string id) where T : class, IStringId;
        Task<object> LoadWithType(Type t, string id);
    }

    public class ClientRepositoryService : IClientRepositoryService
    {
        private ILogService _logService = null;
        private IClientAppService _clientAppService = null;
        private ITextSerializer _serializer = null;

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

        public async Task<bool> Save<T>(T t, RepoSaveArgs args = null) where T : class, IStringId
        {
            try
            {
                IClientRepositoryCollection repo = GetRepositoryFromType(t.GetType());
                return await repo.Save(t, args);
            }
            catch (Exception e)
            {
                Debug.Log("EXC: " + e.Message + " " + e.StackTrace);
            }
            return false;
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

        private Dictionary<Type, object> _repoCache = new Dictionary<Type, object>();
        public IClientRepositoryCollection GetRepositoryFromType(Type t)
        {
            if (_repoCache.TryGetValue(t, out object repo))
            {
                return (IClientRepositoryCollection)repo;
            }

            Type baseRepoType = typeof(ClientRepositoryCollection<>);
            Type genericType = baseRepoType.MakeGenericType(t);
            object newRepo = Activator.CreateInstance(genericType, new object[] { _logService, _clientAppService, _serializer });

            _repoCache[t] = newRepo;

            return (IClientRepositoryCollection)newRepo;
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

    }
}


