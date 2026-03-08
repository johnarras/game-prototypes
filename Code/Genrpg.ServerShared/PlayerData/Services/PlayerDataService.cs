using Genrpg.ServerShared.DataStores;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.DataStores.Categories.PlayerData.Units;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Indexes;
using Genrpg.Shared.DataStores.Interfaces;
using Genrpg.Shared.HelperClasses;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using Genrpg.Shared.Tasks.Services;
using Genrpg.Shared.Units.Loaders;
using Genrpg.Shared.Units.Mappers;
using Genrpg.Shared.Users.Loaders;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.PlayerData.Services
{
    public interface IPlayerDataService : IInitializable
    {
        void SavePlayerData(Character ch);
        Task<List<IUnitData>> MapToClientDto(IFilteredObject obj, List<IUnitData> serverDataList);
        Task<List<IUnitData>> LoadAllPlayerData(IRandom rand, string gameUserId, List<IUnitData> existingData, Character ch = null);
        Task<List<CharacterStub>> LoadCharacterStubs(string userId);
        Dictionary<Type, IUnitDataLoader> GetLoaders();
        IUnitDataLoader GetLoader<T>() where T : IUnitData;

    }
    public class PlayerDataService : IPlayerDataService
    {
        protected IServiceLocator _loc;
        protected IFullRepositoryService _repoService = null;
        protected ITaskService _taskService = null;

        SetupDictionaryContainer<Type, IUnitDataLoader> _loaderObjects = new SetupDictionaryContainer<Type, IUnitDataLoader>();
        SetupDictionaryContainer<Type, IUnitDataMapper> _mapperObjects = new SetupDictionaryContainer<Type, IUnitDataMapper>();
        SetupDictionaryContainer<Type, ISharedUserDataLoader> _sharedObjectLoaders = new SetupDictionaryContainer<Type, ISharedUserDataLoader>();

        public async Task Initialize(CancellationToken token)
        {
            List<IUnitDataLoader> allLoaders = GetLoaders().Values.ToList();

            List<Task> indexTasks = new List<Task>();

            foreach (IUnitDataLoader loader in allLoaders)
            {
                List<CreateIndexData> indexedFields = loader.GetIndexes();

                foreach (CreateIndexData indexedField in indexedFields)
                {
                    indexTasks.Add(_repoService.CreateIndexes(indexedField));
                }
                await Task.WhenAll(indexTasks);
            }

            CreateIndexData data = new CreateIndexData(typeof(CoreCharacter));
            data.Configs.Add(new IndexConfig() { Ascending = true, MemberName = nameof(CoreCharacter.UserId), Unique = false });
            indexTasks.Add(_repoService.CreateIndexes(data));

            await Task.WhenAll(indexTasks);
        }

        public Dictionary<Type, IUnitDataLoader> GetLoaders()
        {
            return _loaderObjects.GetDict();
        }

        public IUnitDataLoader GetLoader<T>() where T : IUnitData
        {
            if (_loaderObjects.TryGetValue(typeof(T), out IUnitDataLoader loader))
            {
                return loader;
            }
            return null;
        }

        public void SavePlayerData(Character ch)
        {
            _repoService.QueueSave(ch);

            List<IUnitData> allData = ch.GetAllData();

            List<IUnitData> nonSearchables = new List<IUnitData>();
            foreach (IUnitData unitData in allData)
            {
                if (unitData is ISearchableItem searchable)
                {
                    _repoService.QueueSave(searchable);
                }
                else
                {
                    nonSearchables.Add(unitData);
                }
            }



            if (nonSearchables.Count > 0)
            {

            }
        }

        public async Task<List<IUnitData>> MapToClientDto(IFilteredObject obj, List<IUnitData> serverDataList)
        {
            List<IUnitData> retval = new List<IUnitData>();

            Version clientVersion = new Version(obj.ClientVersion);
            foreach (IUnitData serverData in serverDataList)
            {
                if (_mapperObjects.TryGetValue(serverData.GetType(), out IUnitDataMapper mapper))
                {
                    if (mapper.SendToClient() &&
                        mapper.GetMinClientVersion() <= clientVersion &&
                        mapper.GetMaxClientVersion() >= clientVersion)
                    {
                        retval.Add(mapper.MapToAPI(serverData));
                    }
                }
            }
            await Task.CompletedTask;
            return retval;
        }

        public async Task<T> LoadTopLevelData<T>(Character ch) where T : class, ITopLevelUnitData, new()
        {
            IUnitDataLoader loader = GetLoader<T>();

            if (loader != null)
            {
                return (T)await loader.LoadTopLevelData(ch);
            }
            return default;
        }

        private async Task CreateDefaultSharedUserData(string gameUserId)
        {
            List<Task> tasks = new List<Task>();
            foreach (ISharedUserDataLoader loader in _sharedObjectLoaders.GetDict().Values)
            {
                tasks.Add(loader.CreateDefaultData(gameUserId));
            }

            await Task.WhenAll(tasks);
        }

        public async Task<List<IUnitData>> LoadAllPlayerData(IRandom rand, string gameUserId, List<IUnitData> existingData, Character ch = null)
        {
            bool haveCharacter = ch != null;

            if (!haveCharacter)
            {
                ch = new Character(new CoreCharacter()) { Id = gameUserId, UserId = gameUserId };
            }

            _taskService.ForgetTask(CreateDefaultSharedUserData(gameUserId), false);

            List<Task<IUnitData>> allTasks = new List<Task<IUnitData>>();
            foreach (IUnitDataLoader loader in _loaderObjects.GetDict().Values)
            {
                if (loader.IsClientOnlyData())
                {
                    continue;
                }

                if (haveCharacter || loader.IsUserData())
                {
                    if (existingData.Any(x => x.GetType() == loader.GetServerType()))
                    {
                        continue;
                    }

                    allTasks.Add(LoadOrCreateData(loader, _repoService, ch));
                }
            }

            IUnitData[] dataArray = await Task.WhenAll(allTasks);

            List<IUnitData> dataList = dataArray.ToList();

            dataList.AddRange(existingData);

            return dataList;
        }

        protected async Task<IUnitData> LoadOrCreateData(IUnitDataLoader loader, IRepositoryService repoSystem, Character ch)
        {
            IUnitData newData = await loader.LoadFullData(ch);
            if (newData == null)
            {
                newData = loader.Create(ch);
            }
            return newData;
        }

        public async Task<List<CharacterStub>> LoadCharacterStubs(string userId)
        {
            // TODO: projection in the repo itself
            List<CoreCharacter> chars = await _repoService.Search<CoreCharacter>(x => x.UserId == userId);

            List<CharacterStub> stubs = new List<CharacterStub>();
            foreach (CoreCharacter ch in chars)
            {
                stubs.Add(new CharacterStub()
                {
                    Id = ch.Id,
                    Name = ch.Name,
                    Level = ch.Level,
                });
            }

            return stubs;
        }

    }
}


