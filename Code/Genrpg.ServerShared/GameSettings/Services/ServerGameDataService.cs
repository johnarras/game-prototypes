using Genrpg.ServerShared.Crypto.Services;
using Genrpg.ServerShared.DataStores;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Characters.Utils;
using Genrpg.Shared.DataStores.Indexes;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.GameSettings.PlayerData;
using Genrpg.Shared.GameSettings.Services;
using Genrpg.Shared.GameSettings.Settings;
using Genrpg.Shared.GameSettings.WebApi.UpdateGameSettings;
using Genrpg.Shared.HelperClasses;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using Genrpg.Shared.PlayerFiltering.Utils;
using Genrpg.Shared.Serialization.Interfaces;
using Genrpg.Shared.SettingsNames.Settings;
using Genrpg.Shared.Time.Services;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Versions.Settings;
using Genrpg.Shared.Website.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.GameSettings.Services
{
    public interface IServerGameDataService : IGameDataService, IInitializable
    {
        Task ReloadGameData();
        List<IGameSettingsLoader> GetAllLoaders();
        Dictionary<Type, IGameSettingsMapper> GetAllMappers();
        bool AcceptedByFilter(IFilteredObject obj, IPlayerFilter filter, DateTime currentTime);
        List<ITopLevelSettings> MapToDto(IFilteredObject obj, List<ITopLevelSettings> startSettings);
        bool SetGameDataOverrides(IFilteredObject fobj, bool forceRefresh);
        List<IWebResponse> GetClientSettings(IFilteredObject fobj, bool forceUpdateOverrides);
    }

    public class ServerGameDataService : IServerGameDataService
    {
        SetupDictionaryContainer<Type, IGameSettingsLoader> _loaderObjects = new SetupDictionaryContainer<Type, IGameSettingsLoader>();
        SetupDictionaryContainer<Type, IGameSettingsMapper> _mapperObjects = new SetupDictionaryContainer<Type, IGameSettingsMapper>();

        protected IFullRepositoryService _repoService = null;
        private IGameData _gameData = null;
        private ICryptoService _cryptoService = null;
        protected ITextSerializer _serializer = null;
        private ITimeService _timeService = null;

        virtual protected bool CreateMissingData => false;

        public virtual async Task Initialize(CancellationToken token)
        {
            List<IGameSettingsLoader> allLoaders = GetAllLoaders();

            List<Task> indexTasks = new List<Task>();

            foreach (IGameSettingsLoader loader in allLoaders)
            {
                List<CreateIndexData> indexedFields = loader.GetIndexes();

                foreach (CreateIndexData indexedField in indexedFields)
                {
                    indexTasks.Add(_repoService.CreateIndexes(indexedField));
                }
                await Task.WhenAll(indexTasks);
            }

            await Task.CompletedTask;
        }



        public List<IGameSettingsLoader> GetAllLoaders()
        {
            return _loaderObjects.GetDict().Values.OrderBy(x => x.GetType().Name).ToList();
        }

        public Dictionary<Type, IGameSettingsMapper> GetAllMappers()
        {
            return _mapperObjects.GetDict();
        }

        public virtual async Task<IGameData> LoadGameData()
        {
            GameData gameData = new GameData();

            List<Task<List<ITopLevelSettings>>> allTasks = new List<Task<List<ITopLevelSettings>>>();

            foreach (IGameSettingsLoader loader in _loaderObjects.GetDict().Values)
            {
                allTasks.Add(loader.LoadAll(_repoService, CreateMissingData));
            }

            List<ITopLevelSettings>[] allSettings = await Task.WhenAll(allTasks.ToArray());

            foreach (List<ITopLevelSettings> settingsList in allSettings)
            {
                foreach (ITopLevelSettings settings in settingsList)
                {
                    gameData.Set(settings);
                }
            }
            gameData.SetupDataDict(true);

            VersionSettings versionSettings = gameData.Get<VersionSettings>(null);

            if (versionSettings == null)
            {
                versionSettings = new VersionSettings() { Id = GameDataConstants.DefaultFilename };
            }

            _gameData.CopyFrom(gameData);

            return gameData;
        }

        public bool SetGameDataOverrides(IFilteredObject obj, bool forceRefresh)
        {

            if (obj == null || obj.DataOverrides == null)
            {
                return true;
            }

            DateTime currentTime = _timeService.GetTime(obj);

            VersionSettings versionSettings = _gameData.Get<VersionSettings>(null);

            DataOverrideSettings dataOverrideSettings = _gameData.Get<DataOverrideSettings>(null);

            SettingsNameSettings settingsNameSettings = _gameData.Get<SettingsNameSettings>(null);

            if (dataOverrideSettings.GetNextUpdateTime(currentTime) <= currentTime)
            {
                dataOverrideSettings.SetPrevNextUpdateTimes(currentTime);
            }

            // If we are not force refreshing, don't always update the settings
            // If the game data wasn't saved and the player's LastTimeSet is after the PrevUpdateTime
            // (most recent override update) and it's before the NextUpdateTime then it means the
            // player has the most recent data and the next data hasn't changed, so don't make 
            // any changes.

            if (!forceRefresh &&
                versionSettings.SaveTime == obj.DataOverrides.GameDataCheckTime &&
                obj.DataOverrides.LastTimeSet >= dataOverrideSettings.GetPrevUpdateTime(currentTime) &&
                obj.DataOverrides.LastTimeSet < dataOverrideSettings.GetNextUpdateTime(currentTime))
            {
                return false;
            }

            List<DataOverrideItemPriority> priorityOverrides = new List<DataOverrideItemPriority>();

            List<DataOverrideGroup> acceptableGroups = new List<DataOverrideGroup>();

            // dataOverrideSettings.GetData() is ordered the DataOverrideSettingsLoader
            foreach (DataOverrideGroup overrideGroup in dataOverrideSettings.GetData())
            {
                if (AcceptedByFilter(obj, overrideGroup, currentTime))
                {
                    // Each group.Items is ordered on load by SettingsId then by DocId
                    foreach (DataOverrideItem groupItem in overrideGroup.Items)
                    {
                        if (groupItem.SettingsNameId < 1 ||
                            string.IsNullOrEmpty(groupItem.DocId) ||
                            !groupItem.Enabled ||
                            groupItem.DocId == GameDataConstants.DefaultFilename)
                        {
                            continue;
                        }

                        DataOverrideItemPriority overrideItem = priorityOverrides.FirstOrDefault(x => x.SettingsNameId == groupItem.SettingsNameId);

                        if (overrideItem == null)
                        {
                            overrideItem = new DataOverrideItemPriority { SettingsNameId = groupItem.SettingsNameId };
                            priorityOverrides.Add(overrideItem);
                            overrideItem.DocId = groupItem.DocId;
                            overrideItem.Priority = overrideGroup.Priority;
                        }
                        else if (overrideGroup.Priority > overrideItem.Priority)
                        {
                            overrideItem.Priority = overrideGroup.Priority;
                            overrideItem.DocId = groupItem.DocId;
                        }
                    }
                }
            }

            obj.DataOverrides.GameDataCheckTime = versionSettings.SaveTime;
            obj.DataOverrides.LastTimeSet = _timeService.GetTime(obj);

            obj.DataOverrides.Items = new List<PlayerSettingsOverrideItem>();

            foreach (DataOverrideItemPriority priority in priorityOverrides)
            {
                obj.DataOverrides.Items.Add(new PlayerSettingsOverrideItem()
                {
                    SettingsNameId = settingsNameSettings.Get(priority.SettingsNameId).IdKey,
                    DocId = priority.DocId,
                });
            }

            obj.DataOverrides.Items = obj.DataOverrides.Items.OrderBy(x => x.SettingsNameId).ToList();

            // This should be deterministic across machines because the player has a set
            // of overrides that should be the same for anyone who's in the same bucket
            // and then the game data save time and the prev update time (last time the
            // overrides changed) will be the same.
            string fullString = _serializer.SerializeToString(obj.DataOverrides.Items) +
                versionSettings.SaveTime.Ticks.ToString() + "." +
                dataOverrideSettings.GetPrevUpdateTime(currentTime).Ticks.ToString();

            obj.DataOverrides.Hash = _cryptoService.QuickHash(fullString);

            if (obj is CoreCharacter coreChar)
            {
                _repoService.QueueSave(coreChar);
            }
            else if (obj is Character realCh)
            {
                CharacterUtils.CopyDataFromTo(realCh, realCh.Core);
                _repoService.QueueSave(realCh.Core);
            }

            return true;
        }

        public List<ITopLevelSettings> MapToDto(IFilteredObject obj, List<ITopLevelSettings> startSettings)
        {
            List<ITopLevelSettings> retval = new List<ITopLevelSettings>();

            Version clientVersion = new Version(obj.ClientVersion);

            foreach (ITopLevelSettings settings in startSettings)
            {
                if (_mapperObjects.TryGetValue(settings.GetType(), out IGameSettingsMapper mapper))
                {
                    if (clientVersion < mapper.GetMinClientVersion() ||
                    clientVersion > mapper.GetMaxClientVersion())
                    {
                        continue;
                    }

                    retval.Add(mapper.MapToDto(settings, true));
                }
                else
                {
                    retval.Add(settings);
                }
            }
            return retval;
        }

        public List<IWebResponse> GetClientSettings(IFilteredObject fobj, bool forceUpdateOverrides)
        {

            List<IWebResponse> retval = new List<IWebResponse>();

            DateTime clientGameDataSaveTime = fobj.DataOverrides.GameDataCheckTime;
            Version clientVersion = new Version(fobj.ClientVersion);
            List<ITopLevelSettings> newSettings = new List<ITopLevelSettings>();

            GameDataOverrideList oldOverrides = fobj.DataOverrides;

            bool didSetOverrides = SetGameDataOverrides(fobj, forceUpdateOverrides);

            GameDataOverrideList newOverrides = fobj.DataOverrides;

            SettingsNameSettings settingsNameSettings = _gameData.Get<SettingsNameSettings>(fobj);

            List<ITopLevelSettings> overrideSettings = _gameData.OverrideSettings();

            List<ITopLevelSettings> orderedDefaultSettings = _gameData.DescendingTimeOrderedDefaultSettings();

            List<IGameSettingsLoader> allLoaders = GetAllLoaders();

            Dictionary<Type, IGameSettingsMapper> mapperDict = _mapperObjects.GetDict();

            foreach (ITopLevelSettings defaultToplevelSetting in orderedDefaultSettings)
            {
                if (defaultToplevelSetting.SaveTime <= clientGameDataSaveTime)
                {
                    break;
                }

                if (mapperDict.TryGetValue(defaultToplevelSetting.GetType(), out IGameSettingsMapper mapper))
                {
                    if (mapper.SendToClient() &&
                        clientVersion >= mapper.GetMinClientVersion() &&
                        clientVersion <= mapper.GetMaxClientVersion())
                    {
                        newSettings.Add(defaultToplevelSetting);
                    }
                }
            }

            if (didSetOverrides)
            {
                foreach (PlayerSettingsOverrideItem item in newOverrides.Items)
                {
                    ITopLevelSettings topLevel = overrideSettings.FirstOrDefault(x =>
                    x.Id == item.DocId && settingsNameSettings.GetIdFromTypeName(x.GetType().Name) == item.SettingsNameId);

                    if (topLevel != null)
                    {
                        newSettings.Add(topLevel);
                    }
                }
            }

            if (didSetOverrides || newSettings.Count > 0)
            {
                UpdateGameSettingsResponse result = new UpdateGameSettingsResponse();
                result.DataOverrides = newOverrides;
                result.NewSettings = MapToDto(fobj, newSettings);

                retval.Add(result);
            }

            fobj.DataOverrides.GameDataCheckTime = _gameData.Get<VersionSettings>(fobj).SaveTime;
            return retval;
        }


        public bool AcceptedByFilter(IFilteredObject obj, IPlayerFilter filter, DateTime currentTime)
        {
            if (!PlayerFilterUtils.IsActive(filter, currentTime))
            {
                return false;
            }

            if (filter.AllowedPlayers.Any(x => x.PlayerId == obj.Id))
            {
                return true;
            }

            if (filter.MinLevel > 0 && filter.MaxLevel > 0 &&
                (filter.MinLevel > obj.Level || filter.MaxLevel < obj.Level))
            {
                return false;
            }

            if (filter.TotalModSize > 0 && filter.MaxModValue > 0)
            {
                long idHash = StrUtils.GetPrefixIdHash(filter.IdKey + obj.Id);

                if (idHash % filter.TotalModSize >= filter.MaxModValue)
                {
                    return false;
                }
            }

            if (filter.MaxInstallDays > 0 && (currentTime - obj.CreationDate).Days > filter.MaxInstallDays)
            {
                return false;
            }

            if (filter.MinInstallDays > 0 && (currentTime - obj.CreationDate).Days < filter.MinInstallDays)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(filter.MinClientVersion) || !string.IsNullOrEmpty(filter.MaxClientVersion))
            {
                if (string.IsNullOrEmpty(obj.ClientVersion))
                {
                    return false;
                }
                Version clientVersion = new Version(obj.ClientVersion);
                if (!string.IsNullOrEmpty(filter.MinClientVersion))
                {
                    Version minVersion = new Version(filter.MinClientVersion);

                    if (clientVersion < minVersion)
                    {
                        return false;
                    }
                }
                if (!string.IsNullOrWhiteSpace(filter.MaxClientVersion))
                {
                    Version maxVersion = new Version(filter.MaxClientVersion);
                    if (clientVersion > maxVersion)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public async Task ReloadGameData()
        {
            IGameData newData = await LoadGameData();
        }
    }
}


