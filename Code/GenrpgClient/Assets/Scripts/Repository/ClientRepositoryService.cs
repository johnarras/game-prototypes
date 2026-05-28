
using Assets.Scripts.Repository.Constants;
using OxDb.SharedCore.DataStores.Entities;
using OxDb.SharedCore.DataStores.Interfaces;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedCore.Serialization.Interfaces;
using OxDb.SharedCore.Setup.Constants;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Repository
{

    public interface IClientRepositoryService : IRepositoryService
    {
        Task<T> LoadObjectFromString<T>(string id) where T : class, IStringId;
        Task<object> LoadWithType(Type t, string id);
        byte[] LoadBytes(string id);
        void SaveBytes(string id, byte[] val, RepoSaveArgs args = null);
        string GetPathPrefix();
    }

    public class ClientRepositoryService : IClientRepositoryService
    {
        private ILogService _logService = null;
        private IClientAppService _clientAppService = null;
        private ITextSerializer _serializer = null;
        private IClientCryptoService _clientCryptoService = null;

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

        public async Task<bool> Save<T>(T t, RepoSaveArgs args = null) where T : IStringId
        {
            try
            {
                IClientRepositoryCollection collection = GetRepositoryFromType(t.GetType());
                return await collection.Save(t, args);
            }
            catch (Exception e)
            {
                _logService.Exception(e, "ClientRepositoryService.Save");
            }
            return false;
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
            // THESE ARGS MUST MATCH THE CONSTRUCTOR ARGS IT HE ClientRepositoryCollection class
            // Look for REPOCREATEARGSYNC to see where the constructor is.
            object newRepo = Activator.CreateInstance(genericType, new object[] { this, _logService, _serializer, _clientCryptoService });

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

        public string GetPathPrefix()
        {
            string prefix = _clientAppService.PersistentDataPath + ClientRepositoryConstants.GetDataPathPrefix();
#if DEMO_BUILD
        if (InitProject.Env != EnvNames.Prod && !string.IsNullOrEmpty(Application.version))
        {
            var version = Application.version.Trim();
            prefix += "V" + version;
        }
#endif
            if (!Directory.Exists(prefix))
            {
                Directory.CreateDirectory(prefix);
            }

            return prefix;
        }

        protected string GetPath(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return "";
            }

            if (id.IndexOf(":") > 0)
            {
                return id;
            }

            int questionMark = id.IndexOf("?");

            if (questionMark > 0)
            {
                id = id.Substring(0, questionMark);
            }

            string basePath = GetPathPrefix();


            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
            }

            if (id.LastIndexOf("/") >= 0)
            {
                string beforeSlash = id.Substring(0, id.LastIndexOf("/"));
                if (!string.IsNullOrEmpty(beforeSlash))
                {
                    string fullDir = basePath + "/" + beforeSlash;
                    if (!Directory.Exists(fullDir))
                    {
                        Directory.CreateDirectory(fullDir);
                    }
                }
            }
            return basePath + "/" + id;
        }


        public byte[] LoadBytes(string id)
        {
            string path = GetPath(id);

            try
            {
                if (!File.Exists(path))
                {
                    return null;
                }
                return File.ReadAllBytes(path);
            }
            catch (Exception e)
            {
                _logService.Info("Failed to read bytes: " + " " + path + " " + e.Message);
            }
            return null;
        }

        public void SaveBytes(string id, byte[] val, RepoSaveArgs args = null)
        {
            if (val == null)
            {
                return;
            }
            string path = GetPath(id);
            try
            {
                File.WriteAllBytes(path, val);
            }
            catch (Exception e)
            {
                _logService.Info("Failed to save bytes: " + path + " " + e.Message);
            }
        }

    }
}


