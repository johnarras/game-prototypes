using Genrpg.ServerShared.Config;
using Genrpg.ServerShared.Core;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils;
using MongoDB.Driver;
using Genrpg.Shared.Website.Interfaces;
using Genrpg.Shared.Users.PlayerData;
using Genrpg.Shared.Website.Messages.Error;
using Genrpg.Shared.DataStores.Categories.PlayerData.Units;
using Genrpg.Shared.DataStores.Categories.PlayerData.Users;
using Genrpg.Shared.Website.Messages;

namespace Genrpg.RequestServer.Core
{

    public class WebContext : ServerGameState
    {
        public User user { get; set; }

        public MyRandom rand { get; set; } = new MyRandom();

        public WebResponseList Responses { get; set; } = new WebResponseList();

        protected IRepositoryService _repoService = null;

        public WebContext(IServerConfig config) : base(config)
        {

        }
        public WebContext(IServerConfig config, IServiceLocator locIn) : base(config)
        {
            loc = locIn;
            rand = new MyRandom();
            _repoService = locIn.Get<IRepositoryService>();
        }

        public async Task<User> LoadUser(string userId)
        {
            if (user == null)
            {
                user = await _repoService.Load<User>(userId);
                Set(user);
            }
            return user;
        }

        protected Dictionary<string, IUnitData> _unitData = new Dictionary<string, IUnitData>();

        public List<IUnitData> GetAllData() { return _unitData.Values.ToList(); }

        public void Set(IUnitData doc)
        {
            string id = doc.Id;
            if (doc is IId iid)
            {
                id = iid.IdKey.ToString();
            }
            _unitData.Add(GetFullKey(doc.GetType(), id), doc);
        }

        public void Remove<T>(string docId) where T : class, IUnitData, new()
        {
            string fullKey = GetFullKey(typeof(T), docId);

            if (_unitData.ContainsKey(fullKey))
            {
                _unitData.Remove(fullKey);
            }
        }

        public T TryGetFromCache<T>(string docId = null) where T : class, IUnitData, new()
        {
            if (string.IsNullOrEmpty(docId))
            {
                docId = user.Id;
            }

            string cacheKey = GetFullKey(typeof(T), docId);

            if (_unitData.ContainsKey(cacheKey))
            {
                return (T)_unitData[cacheKey];
            }
            return null;
        }


        string GetFullKey(Type t, object idObj)
        {
            return (t.Name + idObj.ToString()).ToLower();
        }

        public async Task<T> GetAsync<T>(long idkey) where T : class, IOwnerQuantityChild, new()
        {
            string ownerId = null;
            if (typeof(IUserData).IsAssignableFrom(typeof(T)))
            {
                ownerId = user.Id;
            }
            else if (!string.IsNullOrEmpty(user.CurrCharId))
            {
                ownerId = user.CurrCharId;
            }
            string fullKey = GetFullKey(typeof(T), idkey.ToString());

            if (_unitData.ContainsKey(fullKey))
            {
                return (T)_unitData[fullKey];
            }
            List<T> items = await _repoService.Search<T>(x => x.OwnerId == ownerId && x.IdKey == idkey);

            if (items.Count > 1)
            {
                throw new Exception($"Duplicate player data Item for a given OwnerId {ownerId} and IdKey {idkey})");
            }

            T item = items.FirstOrDefault()!;
            if (item == null)
            {
                item = new T() { Id = HashUtils.NewUUId(), OwnerId = ownerId, IdKey = idkey };
            }

            Set(item);
            return item;
        }

        public async Task<T> GetAsync<T>(string id = null) where T : class, IUnitData, new()
        {
            if (string.IsNullOrEmpty(id))
            {
                if (typeof(IUserData).IsAssignableFrom(typeof(T)))
                {
                    id = user.Id;
                }
                else if (!string.IsNullOrEmpty(user.CurrCharId))
                {
                    id = user.CurrCharId;
                }
                else
                {
                    return default;
                }
            }

            string fullKey = GetFullKey(typeof(T), id);

            if (_unitData.ContainsKey(fullKey))
            {
                return (T)_unitData[fullKey];
            }

            T item = await _repoService.Load<T>(id);

            if (item == null)
            {
                item = new T() { Id = id };
            }
            Set(item);
            return item;
        }

        public void ShowError(string error)
        {
            Responses.AddResponse(new ErrorResponse() { Error = error });
        }


        public async Task SaveAll()
        {
            List<Task> saveTasks = new List<Task>();

            if (user != null)
            {
                saveTasks.Add(_repoService.Save(user));
            }

            List<IUnitData> unitDataList = GetAllData();

            foreach (IUnitData unitData in unitDataList)
            {
                saveTasks.Add(_repoService.Save(unitData));
            }

            await Task.WhenAll(saveTasks);
        }

    }
}
