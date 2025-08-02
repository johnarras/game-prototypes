using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.DataStores.Categories.PlayerData.Units;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Indexes;
using Genrpg.Shared.HelperClasses;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using Genrpg.Shared.Tasks.Services;
using Genrpg.Shared.Units.Loaders;
using Genrpg.Shared.Units.Mappers;
using Genrpg.Shared.Users.Loaders;
using Genrpg.Shared.Users.PlayerData;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.PlayerData
{
    public class PlayerDataService : IPlayerDataService
    {
        protected IServiceLocator _loc;
        protected IRepositoryService _repoService = null;
        protected ITaskService _taskService = null;

        SetupDictionaryContainer<Type,IUnitDataLoader> _loaderObjects = new SetupDictionaryContainer<Type, IUnitDataLoader>();
        SetupDictionaryContainer<Type, IUnitDataMapper> _mapperObjects = new SetupDictionaryContainer<Type, IUnitDataMapper>();
        SetupDictionaryContainer<Type, ISharedUserDataLoader> _sharedObjectLoaders = new SetupDictionaryContainer<Type, ISharedUserDataLoader>();

        public async Task Initialize(CancellationToken token)
        {
            List<Task> loaderTasks = new List<Task>();
            CreateIndexData data = new CreateIndexData();
           // data.Configs.Add(new IndexConfig() { Ascending = true, MemberName = nameof(CoreCharacter.UserId), Unique = false });
           // await _repoService.CreateIndex<CoreCharacter>(data);
            await Task.CompletedTask;
        }

        public Dictionary<Type,IUnitDataLoader> GetLoaders()
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

        public void SavePlayerData(Character ch, bool saveAll)
        {
            ch?.SaveData(_repoService, saveAll);
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
                else
                {
                    Console.WriteLine("Missing mapper: " + serverData.GetType().Name);
                }
            }
            await Task.CompletedTask;
            return retval;
        }

        public async Task<T> LoadTopLevelData<T> (Character ch) where T : class, ITopLevelUnitData, new()
        { 
            IUnitDataLoader loader = GetLoader<T>();

            if (loader != null)
            {
                return (T)await loader.LoadTopLevelData(ch);
            }
            return default;
        }

        private async Task CreateDefaultSharedUserData(User user)
        {
            List<Task> tasks = new List<Task>();
            foreach (ISharedUserDataLoader loader in _sharedObjectLoaders.GetDict().Values)
            {
                tasks.Add(loader.CreateDefaultData(user.Id));
            }

            await Task.WhenAll(tasks);
        }

        public async Task<List<IUnitData>> LoadAllPlayerData(IRandom rand, User user, Character ch = null)
        {
            bool haveCharacter = ch != null;

            if (!haveCharacter)
            {
                ch = new Character(new CoreCharacter()) { Id = user.Id, UserId = user.Id };
            }

            _taskService.ForgetTask(CreateDefaultSharedUserData(user), false);

            List<Task<IUnitData>> allTasks = new List<Task<IUnitData>>();
            foreach (IUnitDataLoader loader in _loaderObjects.GetDict().Values)
            {
                if (haveCharacter || loader.IsUserData())
                {
                    allTasks.Add(LoadOrCreateData(loader, _repoService, ch));
                }
            }

            IUnitData[] dataArray = await Task.WhenAll(allTasks);

            List<IUnitData> dataList = dataArray.ToList();
           
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
